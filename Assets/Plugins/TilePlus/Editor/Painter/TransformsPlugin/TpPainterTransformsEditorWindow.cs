// ***********************************************************************
// Assembly         : TilePlus.Editor
// Author           : Jeff Sasmor
// Created          : 01-01-2023
//
// Last Modified By : Jeff Sasmor
// Last Modified On : 01-05-2023
// ***********************************************************************
// <copyright file="TpPainterTransformsEditorWindow.cs" company="Jeff Sasmor">
//     Copyright (c) Jeff Sasmor. All rights reserved.
// </copyright>
// ***********************************************************************
#nullable disable
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.UIElements;
using static TilePlus.Editor.TpIconLib;
using Button = UnityEngine.UIElements.Button;

namespace TilePlus.Editor.Painter
{

    /// <summary>
    /// An Editor Window for the TpPainterTransforms Scriptable Object
    /// </summary>
    public class TpPainterTransformsEditorWindow : EditorWindow
    {
        private static TpPainterTransformsEditorWindow s_Instance;

        /// <summary>
        /// The editor Window instance
        /// </summary>
        public static TpPainterTransformsEditorWindow Instance => s_Instance;

        private TpListView                            listView;

        private const float ListItemHeight = 16;
        
        private enum TransformComponent
        {
            Position,
            Rotation,
            Scale
        }

        /// <summary>
        /// Open the TilePlusViewer window
        /// </summary>
        [Shortcut("TilePlus/Painter:Open Transform Editor", KeyCode.Alpha3, ShortcutModifiers.Alt)]
        [MenuItem("Tools/TilePlus/Tile+Painter Transform Editor", false, 2)]
        public static void ShowWindow()
        {
            s_Instance              = GetWindow<TpPainterTransformsEditorWindow>();
            s_Instance.titleContent = new GUIContent("Tile+Painter Transform Editor", FindIcon(TpIconType.TptIcon));
            s_Instance.minSize      = new Vector2(256 , 384);
        }


        

        private void SetSelectionInAsset(int selection)
        {
            TpPainterTransforms.instance.m_ActiveIndex = selection;
            TpPainterTransforms.instance.SaveData();
        }

        
        
        private void CreateGUI()
        {
            rootVisualElement.Clear();
            if (TpPainterTransforms.instance == null)
            {
                rootVisualElement.Add(new TpHelpBox("TpPainterTransforms asset not found!","error-message",HelpBoxMessageType.Error));
                return;
            }

            var container = new VisualElement() {name="outer-container",  style = { flexGrow = 1 } };
            rootVisualElement.Add(container);
            
            listView = new TpListView(ListItems,
                                      ListItemHeight,
                                      true,
                                      MakeTrItem,
                                      BindTrItem)
                       { 
                           style =
                           {
                               flexGrow = 1
                           }
                       };
            listView.Q<VisualElement>("unity-content-container").style.flexGrow = 1;
            
            
            container.Add(listView);
            container.Add(new TpSpacer(10,10));
            var button = new Button(AddItemEvent)
                         {
                             style =
                             {
                                 flexGrow = 0
                             },
                             text    = "Add...",
                             tooltip = "Click to add a new item"
                         };
            container.Add(button);
            
        }

        

        private void AddItemEvent()
        {
            TpPainterTransforms.instance.m_PTransformsList.Add(new TileTransformWrapper());
            TpPainterTransforms.instance.SaveData();
            listView.Rebuild();

        }


        private List<TileTransformWrapper> ListItems => TpPainterTransforms.instance.m_PTransformsList;

        /// <summary>
        /// List item
        /// </summary>
        /// <returns></returns>
        // ReSharper disable once AnnotateNotNullTypeMember
        private VisualElement MakeTrItem()
        {
            //container
            var ve = new VisualElement(){name="element-container", style=
                                        {
                                            minHeight = ListItemHeight * 4, flexDirection = FlexDirection.Row,
                                            
                                        }};
            // ReSharper disable once HeapView.CanAvoidClosure
            ve.RegisterCallback<ClickEvent>(_ =>
                                            {
                                                var selection = (int)ve.userData;
                                                listView.SetSelectionWithoutNotify(new[]{selection});
                                                SetSelectionInAsset(selection);

                                            });
            //transform fields
            var container = new VisualElement()
                            {
                                name = "transform-container",
                                style =
                                {
                                    minHeight = ListItemHeight * 4,
                                    flexGrow  = 1,
                                    alignSelf = Align.FlexStart
                                }
                            };
            var posField = new Vector3Field("Position"){name ="position"};
            posField.RegisterCallback<ChangeEvent<Vector3>>((evt)=>ChangeItem(TransformComponent.Position, evt.newValue));
            var rotField = new Vector3Field("Rotation"){name ="rotation"};
            rotField.RegisterCallback<ChangeEvent<Vector3>>((evt)=>ChangeItem(TransformComponent.Rotation, evt.newValue));
            var scaleField = new Vector3Field("Scale"){name    ="scale"};
            scaleField.RegisterCallback<ChangeEvent<Vector3>>((evt)=>ChangeItem(TransformComponent.Scale, evt.newValue));

            container.Add(posField);
            container.Add(rotField);
            container.Add(scaleField);


            var buttonImage = TpIconLib.FindIcon(TpIconType.UnityXIcon);
            //var h           = buttonImage.height;
            //var w           = buttonImage.width;
            
            var button = new Button(DeleteItemClickEvent) {  name = "delete",
                                                        tooltip = "Delete this entry",
                                                        
                                                        style =
                                                    {
                                                        backgroundImage = buttonImage,
                                                        alignSelf = Align.Center,
                                                        flexGrow = 0,
                                                        height = 15,
                                                        width = 15
                                                    } };
            
            ve.Add(container);
            ve.Add(button);

            return ve;


            

            void ChangeItem(TransformComponent which, Vector3 value)
            {
                var index     = (int)ve.userData;
                if (index > TpPainterTransforms.instance.m_PTransformsList.Count)
                    return;
                var transform = TpPainterTransforms.instance.m_PTransformsList[index].m_Matrix;
                GetTransformComponents(transform,out var pos, out var rot, out var scale);
                if (which == TransformComponent.Position)
                    TpPainterTransforms.instance.m_PTransformsList[index].m_Matrix = Matrix4x4.TRS(value, Quaternion.Euler(rot), scale);
                else if(which == TransformComponent.Rotation)
                    TpPainterTransforms.instance.m_PTransformsList[index].m_Matrix = Matrix4x4.TRS(pos, Quaternion.Euler(value), scale);
                else if(which == TransformComponent.Scale)
                    TpPainterTransforms.instance.m_PTransformsList[index].m_Matrix = Matrix4x4.TRS(pos, Quaternion.Euler(rot), value);
                TpPainterTransforms.instance.SaveData();
                
            }
                
            void DeleteItemClickEvent()
            {
                if (TpPainterTransforms.instance.m_PTransformsList.Count < 2)
                {
                    TpLib.DelayedCallback(this,() =>
                                               {
                                                   EditorUtility.DisplayDialog("Sorry!", "Can't delete the only entry!", "Move on...");

                                               },"T+TE: cant-delete");
                    return;
                }
                var index = (int)ve.userData;
                TpPainterTransforms.instance.m_PTransformsList.RemoveAt(index);
                TpPainterTransforms.instance.m_ActiveIndex = 0;
                TpPainterTransforms.instance.SaveData();
                
                listView.Rebuild();
                listView.SetSelectionWithoutNotify(new []{0});
            }
            
            void GetTransformComponents(Matrix4x4   transform,
                                        out Vector3 tPosition,
                                        out Vector3 tRotation,
                                        out Vector3 tScale)
            {
                tPosition = transform.GetPosition();
                tRotation = transform.rotation.eulerAngles;
                tScale    = transform.lossyScale;
            }
            
            
            

        }



        // ReSharper disable once AnnotateNotNullParameter
        private void BindTrItem(VisualElement ve, int index)
        {
            var posField = ve.Q<Vector3Field>("position");
            var rotField = ve.Q<Vector3Field>("rotation");
            var scaleField = ve.Q<Vector3Field>("scale");
            posField.userData   = index;
            rotField.userData   = index;
            scaleField.userData = index;

            var item = ListItems[index].m_Matrix;
            ve.userData = index;
            
            var pos   = item.GetPosition();
            var rot   = item.rotation.eulerAngles;
            var scale = item.lossyScale;
            
            var rPos = TileUtil.RoundVector3(pos,4);
            var rRotation         = TileUtil.RoundVector3(rot, 4);
            var rScale            = TileUtil.RoundVector3(scale,    4);

            posField.value   = rPos;
            rotField.value   = rRotation;
            scaleField.value = rScale;



        }

    }
}
