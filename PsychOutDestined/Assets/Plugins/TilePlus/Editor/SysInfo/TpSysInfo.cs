// ***********************************************************************
// Assembly         : TilePlus.Editor
// Author           : Jeff Sasmor
// Created          : 01-01-2023
//
// Last Modified By : Jeff Sasmor
// Last Modified On : 12-27-2022
// ***********************************************************************
// <copyright file="TpSysInfo.cs" company="Jeff Sasmor">
//     Copyright (c) Jeff Sasmor. All rights reserved.
// </copyright>
// <summary>A simple editor window to show TilePlus Toolkit status</summary>
// ***********************************************************************

using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UIElements;
// ReSharper disable AnnotateNotNullTypeMember

namespace TilePlus.Editor
{
    /// <summary>
    /// Editor window used to show TilePlus system info
    /// </summary>
    public class TpSysInfo : EditorWindow
    {
        private TextField content;
        
        /// <summary>
        /// Open this editor window
        /// </summary>
        [MenuItem("Tools/TilePlus/System Info", false, 20)] 
        [Shortcut("TilePlus/Open System Info", KeyCode.Alpha2, ShortcutModifiers.Alt)]
        public static void ShowWindow()
        {
            var wnd = GetWindow<TpSysInfo>();
            wnd.titleContent = new GUIContent(Text.Stats_Title);
            wnd.minSize      = new Vector2(100, 100);
        }

        /// <summary>
        /// UIElements CreateGUI callback
        /// </summary>
        public void CreateGUI()
        {
            var spacer = new VisualElement
            {
                name = "spacer",
                style = 
                {
                    height = 10,
                    width = 10,
                    minHeight = 10,
                    minWidth = 10
                }
                
            };
            
            rootVisualElement.Add(spacer);
            rootVisualElement.Add(new Label(Text.Stats_OnPlayWarning));
            content = new TextField
                      {
                          focusable = false, multiline = true, name = "setup-sysinfo-textfield", style =
                          {
                              
                              marginTop = 10,
                              marginBottom = 10,
                              whiteSpace = WhiteSpace.Normal,
                              flexGrow                = 1,
                              flexShrink = 1,
                              //flexWrap = Wrap.Wrap,
                              unityFontStyleAndWeight = FontStyle.Bold
                          },
                          value = SysInfoText()
                      };

            #if UNITY_2023_1_OR_NEWER
            content.verticalScrollerVisibility = ScrollerVisibility.Auto;
            #else
            content.SetVerticalScrollerVisibility(ScrollerVisibility.Auto);
            #endif
            
            rootVisualElement.Add(content);
        }

        private void OnInspectorUpdate()
        {
            if(content != null) //can happen after a scripting reload.
                content.value = SysInfoText();
        }

        private long maxMem;

        private string SysInfoText()
        {
            const int limit = 50;
            var mem = Profiler.usedHeapSizeLong;
            if (mem > maxMem)
                maxMem = mem;

            var maxClonesPerUpdate   = TpLib.MaxNumClonesPerUpdate;
            var maxCallbackPerUpdate = TpLib.MaxNumDeferredCallbacksPerUpdate;

            var zmOn          = TileFabLib.AnyActiveZoneManagers ? "[ON]" :"[OFF]";
            var zmNumRegs     = TileFabLib.RegistrationIndex - 1; //-1 because index starts at 1. 
            var zmTotalChunks = TileFabLib.NumZoneManagerChunks;
            var zmTotalZms    = TileFabLib.NumZoneManagerInstances;
            
            return $"{TpLib.VersionInformation}\n{TpLibEditor.VersionInformation}\n\n"
                   + $"Heap Memory: {mem:N0}, Max: {maxMem:N0} \n"
                   + $"TpLib Initial Mem Alloc: {TpLib.MemAllocSettings}\n\n"
                   + $"TpLib:  {TpLib.TilemapsCount} tilemap(s), \n{TpLib.TaggedTilesCount} tag(s)\n{TpLib.TileTypesCount} type(s),\n{TpLib.TileInterfacesCount} interface(s),\n{TpLib.GuidToTileCount} GUID(s)."
                   + $"\n\n{Text.Stats_TpWarning}\n\n"
                   + $"TpLib internal pools:\nTPB:{TpLib.TpbPoolStat}\nDicts: {TpLib.s_DictOfV3IToTpb_PoolStat}\nDeferredEvents: {TpLib.DeferredEvtPoolStat}\nList_Tilemap: {TpLib.ListTilemapPoolStat}\nCloning_Data: {TpLib.CloningDataPoolStat}\nDeferredCallback {TpLib.DeferredCallbackPoolStat}\n\n"
                   + $"SpawnUtil: Pooled prefab parent: {SpawningUtil.CleanPoolHostName}\n{SpawningUtil.PoolStatus(limit)}\n\n"
                   + $"Events: Save:{TpEvents.NumSaveEvents}, Trigger:{TpEvents.NumTriggerEvents}\n\n"
                   + $"Conditional Async callbacks (EditorOnly): {TpConditionalTasks.ConditionalTaskInfo}\n\n"
                   + $"TpLib Delayed Async callbacks: {TpLib.CurrentActiveDelayedCallbacks.ToString()}\n\n"
                   + $"TpLib Cloning Queue Depth: {TpLib.CloneQueueDepth}, Max Depth:  {TpLib.CloneQueueMaxDepth}\nNum of clones per Update: {maxClonesPerUpdate} (unlimited when not in Play)\n\n"
                   + $"TpLib Callback Queue Depth: {TpLib.DeferredQueueDepth}, Max Depth:  {TpLib.DeferredQueueMaxDepth}\nNum of callbacks per Update: {maxCallbackPerUpdate} (unlimited when not in Play)\n\n"
                   + $"TileFabLib: ZoneManager {zmOn}, #Instances:{zmTotalZms}, {zmNumRegs} Regs, {zmTotalChunks} total managed Chunks.\n";

        }
    
        

        private static class Text
        {
            public const string Stats_Title         = "Tile+Toolkit System Information";
            public const string Stats_OnPlayWarning = "This panel refreshes automatically, even in Play mode.";
            public const string Stats_TpWarning     = "The above values refer to TilePlus tiles only, and do not include normal Unity tiles.";

        }
    }
}
