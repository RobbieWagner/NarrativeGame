using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

namespace RobbieWagnerGames.UI
{
    public class MenuWithTabs : Menu
    {
        [SerializeField] public bool navigateMenuHorizontally = true;

        [SerializeField] protected HorizontalLayoutGroup tabBar;
        protected List<TextMeshProUGUI> tabBarTextObjects; 
        [SerializeField] protected TextMeshProUGUI tabNamePrefab;
        [SerializeField] protected Color inactiveColor;
        [SerializeField] protected Color activeColor;

        [SerializeField] protected List<MenuTab> submenuPrefabs;
        protected List<MenuTab> instantiatedSubmenus;

        private int activeTab = -1;
        public int ActiveTab
        {
            get { return activeTab; }
            set 
            {
                if(value == activeTab || instantiatedSubmenus.Count == 0) return;
                DisableActiveTab();

                activeTab = value;
                if(activeTab < 0)
                {
                    activeTab = instantiatedSubmenus.Count - 1;
                }
                else if(activeTab >= instantiatedSubmenus.Count) 
                {
                    activeTab = 0;
                }

                EnableTab(activeTab);
            }
        }

    protected override void OnEnable()
        {
            BuildMenu();
            base.OnEnable();
            ActiveTab = 0;
            EnableTab(ActiveTab);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        protected virtual void BuildMenu()
        {
            //Remove the old menu instantiation
            if(instantiatedSubmenus != null)
            {
                foreach(MenuTab tab in instantiatedSubmenus)
                {
                    Destroy(tab.gameObject);
                }

                instantiatedSubmenus.Clear();
            }
            else
            {
                instantiatedSubmenus = new List<MenuTab>();
            }

            if(tabBarTextObjects != null)
            {
                foreach(TextMeshProUGUI tabText in tabBarTextObjects)
                {
                    Destroy(tabText.gameObject);
                }

                tabBarTextObjects.Clear();
            }
            else
            {
                tabBarTextObjects = new List<TextMeshProUGUI>();
            }

            //Create the new tabs, and place them into the scene
            foreach(MenuTab tab in submenuPrefabs)
            {
                MenuTab newTab = Instantiate(tab, this.transform).GetComponent<MenuTab>();
                instantiatedSubmenus.Add(newTab);
                newTab.BuildTab();
                newTab.gameObject.SetActive(false);

                TextMeshProUGUI tabNameText = Instantiate(tabNamePrefab, tabBar.transform).GetComponent<TextMeshProUGUI>();
                tabBarTextObjects.Add(tabNameText);
                tabNameText.color = inactiveColor;
                tabNameText.text = newTab.tabName;
            }
        }

        public virtual void OnNavigateMenuHorizontally(InputValue inputValue)
        {
            if(navigateMenuHorizontally && this.enabled)
            {
                float value = inputValue.Get<float>();
                if(value > 0) 
                {
                    ActiveTab++;
                }
                else if(value < 0) 
                {
                    ActiveTab--;
                }
            }
        }

        public virtual void OnNavigateMenuVertically(InputValue inputValue)
        {
            if(!navigateMenuHorizontally && this.enabled)
            {
                float value = inputValue.Get<float>();
                if(value > 0) 
                {
                    ActiveTab++;
                }
                else if(value < 0) 
                {
                    ActiveTab--;
                }
            }
        }

        public virtual void EnableTab(int tab)
        {
            instantiatedSubmenus[tab].gameObject.SetActive(true);
            tabBarTextObjects[tab].color = activeColor;
        }

        public virtual void DisableActiveTab()
        {
            if(ActiveTab > -1 && ActiveTab < instantiatedSubmenus.Count)
            {
                instantiatedSubmenus[ActiveTab].gameObject.SetActive(false);
                tabBarTextObjects[ActiveTab].color = inactiveColor;
            }
        }
    }
}