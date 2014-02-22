using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Kerberos.Sots;
using Kerberos.Sots.Data;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;

using Original = Kerberos.Sots.UI;
using PerformanceData = Bardez.Projects.SwordOfTheStars.SotS2.Fixes.Sots.Data;
using PerformanceStrategy = Bardez.Projects.SwordOfTheStars.SotS2.Fixes.Sots.Strategy;

namespace Bardez.Projects.SwordOfTheStars.SotS2.Fixes.Sots.UI
{
    /// <summary>Contains performance fixes relative to Kerberos reflected original</summary>
	internal class FleetWidget
    {
        #region Local support class(es)
        /// <summary>Wrapper for Kerberos.Sots.UI.FleetWidget which exposes its properties and methods for local consumption</summary>
        protected class WidgetWrapper
        {
            #region Fields
            /// <summary>Widget to use as reference</summary>
            public Original.FleetWidget BaseWidget { get; set;}
            #endregion


            #region Widget's Reflection-Exposed Properties
            public Boolean _contentChanged
            {
                get { return WidgetWrapper.PrivateField<Boolean>(this.BaseWidget, "_contentChanged"); }
                set { WidgetWrapper.PrivateField<Boolean>(this.BaseWidget, "_contentChanged", value); }
            }

            public Boolean _ready
            {
                get { return WidgetWrapper.PrivateField<Boolean>(this.BaseWidget, "_ready"); }
            }

            public Boolean _jumboMode
            {
                get { return WidgetWrapper.PrivateField<Boolean>(this.BaseWidget, "_jumboMode"); }
            }

            public Boolean _enemySelectionEnabled
            {
                get { return WidgetWrapper.PrivateField<Boolean>(this.BaseWidget, "_enemySelectionEnabled"); }
            }

            public Boolean _scrapEnabled
            {
                get { return WidgetWrapper.PrivateField<Boolean>(this.BaseWidget, "_scrapEnabled"); }
            }

            public Boolean _fleetsChanged
            {
                get { return WidgetWrapper.PrivateField<Boolean>(this.BaseWidget, "_fleetsChanged"); }
                set { WidgetWrapper.PrivateField<Boolean>(this.BaseWidget, "_fleetsChanged", value); }
            }

            public Boolean _listStations
            {
                get { return WidgetWrapper.PrivateField<Boolean>(this.BaseWidget, "_listStations"); }
            }

            public Boolean _planetsChanged
            {
                get { return WidgetWrapper.PrivateField<Boolean>(this.BaseWidget, "_planetsChanged"); }
                set { WidgetWrapper.PrivateField<Boolean>(this.BaseWidget, "_planetsChanged", value); }
            }

            public Boolean _showFleetInfo
            {
                get { return WidgetWrapper.PrivateField<Boolean>(this.BaseWidget, "_showFleetInfo"); }
            }

            public Boolean _shipSelectionEnabled
            {
                get { return WidgetWrapper.PrivateField<Boolean>(this.BaseWidget, "_shipSelectionEnabled"); }
            }

            public Boolean _preferredSelectMode
            {
                get { return WidgetWrapper.PrivateField<Boolean>(this.BaseWidget, "_preferredSelectMode"); }
            }

            public Boolean _hasSuulka
            {
                get { return WidgetWrapper.PrivateField<Boolean>(this.BaseWidget, "_hasSuulka"); }
            }

            public Boolean _repairModeChanged
            {
                get { return WidgetWrapper.PrivateField<Boolean>(this.BaseWidget, "_repairModeChanged"); }
                set { WidgetWrapper.PrivateField<Boolean>(this.BaseWidget, "_repairModeChanged", value); }
            }

            public Boolean _repairMode
            {
                get { return WidgetWrapper.PrivateField<Boolean>(this.BaseWidget, "_repairMode"); }
            }

            public Boolean _showRepairPoints
            {
                get { return WidgetWrapper.PrivateField<Boolean>(this.BaseWidget, "_showRepairPoints"); }
            }

            public Boolean _expandAll
            {
                get { return WidgetWrapper.PrivateField<Boolean>(this.BaseWidget, "_expandAll"); }
                set { WidgetWrapper.PrivateField<Boolean>(this.BaseWidget, "_expandAll", value); }
            }

            public Boolean _repairAll
            {
                get { return WidgetWrapper.PrivateField<Boolean>(this.BaseWidget, "_repairAll"); }
                set { WidgetWrapper.PrivateField<Boolean>(this.BaseWidget, "_repairAll", value); }
            }

            public Boolean _undoAll
            {
                get { return WidgetWrapper.PrivateField<Boolean>(this.BaseWidget, "_undoAll"); }
                set { WidgetWrapper.PrivateField<Boolean>(this.BaseWidget, "_undoAll", value); }
            }

            public Boolean _enabled
            {
                get { return WidgetWrapper.PrivateField<Boolean>(this.BaseWidget, "_enabled"); }
            }

            public Boolean _confirmRepairs
            {
                get { return WidgetWrapper.PrivateField<Boolean>(this.BaseWidget, "_confirmRepairs"); }
                set { WidgetWrapper.PrivateField<Boolean>(this.BaseWidget, "_confirmRepairs", value); }
            }

            public Boolean _showEmptyFleets
            {
                get { return WidgetWrapper.PrivateField<Boolean>(this.BaseWidget, "_showEmptyFleets"); }
            }

            public Boolean _onlyLocalPlayer
            {
                get { return WidgetWrapper.PrivateField<Boolean>(this.BaseWidget, "_onlyLocalPlayer"); }
            }

            public Boolean _showColonies
            {
                get { return WidgetWrapper.PrivateField<Boolean>(this.BaseWidget, "_showColonies"); }
            }

            public Int32 _selected
            {
                get { return WidgetWrapper.PrivateField<Int32>(this.BaseWidget, "_selected"); }
            }

            public MissionType _missionMode
            {
                get { return WidgetWrapper.PrivateField<MissionType>(this.BaseWidget, "_missionMode"); }
            }

            public Boolean SuulkaMode
            {
                get { return WidgetWrapper.PrivateField<Boolean>(this.BaseWidget, "_suulkaMode"); }
                set { WidgetWrapper.PrivateField<Boolean>(this.BaseWidget, "_suulkaMode", value); }
            }

            public Boolean ShowPiracyFleets
            {
                get { return WidgetWrapper.PrivateField<Boolean>(this.BaseWidget, "_ShowPiracyFleets"); }
                set { WidgetWrapper.PrivateField<Boolean>(this.BaseWidget, "_ShowPiracyFleets", value); }
            }

            public App App
            {
                get { return this.BaseWidget.App; }
            }

            //This seems a bit redundant, no?
            public App _game
            {
                get { return WidgetWrapper.PrivateField<App>(this.BaseWidget, "_game"); }
            }

            public Original.FleetWidget _repairWidget
            {
                get { return WidgetWrapper.PrivateField<Original.FleetWidget>(this.BaseWidget, "_repairWidget"); }
            }

            public List<Int32> AccountedSystems
            {
                get { return WidgetWrapper.PrivateField<List<Int32>>(this.BaseWidget, "AccountedSystems"); }
            }

            public List<Int32> _syncedFleets
            {
                get { return WidgetWrapper.PrivateField<List<Int32>>(this.BaseWidget, "_syncedFleets"); }
            }

            public List<Int32> _syncedStations
            {
                get { return WidgetWrapper.PrivateField<List<Int32>>(this.BaseWidget, "_syncedStations"); }
            }
            #endregion


            #region Kerberos' FleetWidget event exposure
            public MulticastDelegate ShipFilter
            {
                get
                {
                    return (MulticastDelegate)typeof(Original.FleetWidget).GetField("ShipFilter", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy).GetValue(this.BaseWidget);
                }
            }
            #endregion


            #region Construction
            /// <summary>Definition constructor</summary>
            /// <param name="widget">Kerberos FleetWidget being wrapped</param>
            internal WidgetWrapper(Original.FleetWidget widget)
            {
                this.BaseWidget = widget;
            }
            #endregion


            #region Kerberos' FleetWidget public methods (some are shortcutted from private that implement public calls)
            public void SetMissionMode(Boolean flag)
            {
                this.BaseWidget.PostSetProp("MissionMode", flag);
            }

            public void ClearFleets()
            {
                this.BaseWidget.PostSetProp("ClearFleets", new Object[0]);
            }

            public void ClearPlanets()
            {
                this.BaseWidget.PostSetProp("ClearPlanets", new Object[0]);
            }

            public void PostSetProp(String propertyName, Boolean value)
            {
                this.BaseWidget.PostSetProp(propertyName, value);
            }

            public void PostSetProp(String propertyName, Int32 value)
            {
                this.BaseWidget.PostSetProp(propertyName, value);
            }

            public void PostSetProp(String propertyName, IGameObject value)
            {
                this.BaseWidget.PostSetProp(propertyName, value);
            }

            public void PostSetProp(String propertyName, params Object[] values)
            {
                this.BaseWidget.PostSetProp(propertyName, values);
            }

            public void SyncFleetInfo(FleetInfo fleet)
            {
                this.BaseWidget.SyncFleetInfo(fleet);
            }
            #endregion


            #region Kerberos' FleetWidget Reflection-Exposed Methods
            public void SyncSystem(StarSystemInfo system)
            {
                MethodInfo refMethod = WidgetWrapper.PrivateMethod(this.BaseWidget, "SyncSystem");
                refMethod.Invoke(this.BaseWidget, new Object[] { system });
            }

            public void SyncStations(List<StationInfo> stationInfos)
            {
                MethodInfo refMethod = WidgetWrapper.PrivateMethod(this.BaseWidget, "SyncStations");
                refMethod.Invoke(this.BaseWidget, new Object[] { stationInfos });
            }

            public void SyncPlanets()
            {
                MethodInfo refMethod = WidgetWrapper.PrivateMethod(this.BaseWidget, "SyncPlanets");
                refMethod.Invoke(this.BaseWidget, new Object[] { });
            }

            public void SetShowEmptyFleets(Boolean val)
            {
                MethodInfo refMethod = WidgetWrapper.PrivateMethod(this.BaseWidget, "SetShowEmptyFleets");
                refMethod.Invoke(this.BaseWidget, new Object[] { val });
            }

            public void SyncSuulkas(FleetInfo fleet)
            {
                MethodInfo refMethod = WidgetWrapper.PrivateMethod(this.BaseWidget, "SyncSuulkas");
                refMethod.Invoke(this.BaseWidget, new Object[] { fleet });
            }
            #endregion


            #region Helper Methods
            /// <summary>Exposes a FleetWidget's private field</summary>
            /// <typeparam name="T">Type of value to return</typeparam>
            /// <param name="widget">FleetWidget to extract from</param>
            /// <param name="fieldName">Name of the field to expose</param>
            /// <returns>The extracted value</returns>
            private static T PrivateField<T>(Original.FleetWidget widget, String fieldName)
            {
                //HACK: use reflection to access a private type.
                T privateVariable = default(T);
                FieldInfo privateField = typeof(Original.FleetWidget).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                if (privateField == null)
                    throw new NullReferenceException(String.Format("Could not retrieve the \"{0}\" FieldInfo for the Kerberos FleetWidget.", fieldName));
                else
                    privateVariable = (T)(privateField.GetValue(widget));

                return privateVariable;
            }

            /// <summary>Sets a FleetWidget's private field</summary>
            /// <typeparam name="T">Type of value to return</typeparam>
            /// <param name="widget">FleetWidget to extract from</param>
            /// <param name="fieldName">Name of the field to expose</param>
            /// <param name="value">Value to set</param>
            /// <returns>The extracted value</returns>
            private static void PrivateField<T>(Original.FleetWidget widget, String fieldName, T value)
            {
                //HACK: use reflection to access a private type.
                FieldInfo privateField = typeof(Original.FleetWidget).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                if (privateField == null)
                    throw new NullReferenceException(String.Format("Could not retrieve the \"{0}\" FieldInfo for the Kerberos FleetWidget.", fieldName));

                privateField.SetValue(widget, value);
            }

            /// <summary>Exposes a FleetWidget's private property</summary>
            /// <typeparam name="T">Type of value to return</typeparam>
            /// <param name="widget">FleetWidget to extract from</param>
            /// <param name="fieldName">Name of the property to expose</param>
            /// <returns>The extracted value</returns>
            private static T PrivateProperty<T>(Original.FleetWidget widget, String propertyName)
            {
                //HACK: use reflection to access a private method.
                T privateVariable = default(T);
                PropertyInfo privateProperty = typeof(Original.FleetWidget).GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                if (privateProperty == null)
                    throw new NullReferenceException(String.Format("Could not retrieve the \"{0}\" PropertyInfo for the Kerberos FleetWidget.", propertyName));
                else
                {
                    MethodInfo mi = privateProperty.GetGetMethod(true);
                    privateVariable = (T)(mi.Invoke(widget, new Object[] { widget } ));
                }

                return privateVariable;
            }

            private static MethodInfo PrivateMethod(Original.FleetWidget widget, String methodName)
            {
                //HACK: use reflection to access a private method.
                MethodInfo privateMethod = typeof(Original.FleetWidget).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                if (privateMethod == null)
                    throw new NullReferenceException(String.Format("Could not retrieve the \"{0}\" MethodInfo for the Kerberos FleetWidget.", methodName));

                return privateMethod;
            }

            private static EventInfo PrivateEvent(Original.FleetWidget widget, String propertyName)
            {
                //HACK: use reflection to access a private method.
                EventInfo privateEvent = typeof(Original.FleetWidget).GetEvent(propertyName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                if (privateEvent == null)
                    throw new NullReferenceException(String.Format("Could not retrieve the \"{0}\" EventInfo for the Kerberos FleetWidget.", propertyName));

                return privateEvent;
            }
            #endregion
        }
        #endregion


        #region Fields
        /// <summary>Instance of the wrapper around the Kerberos FleetWidget access/exposure</summary>
        protected WidgetWrapper Widget;
        #endregion


        #region Construction
        /// <summary>Definition constructor</summary>
        /// <param name="widget">KErberos FleetWidget to access and such</param>
        public FleetWidget(Original.FleetWidget widget)
        {
            this.Widget = new WidgetWrapper(widget);
        }
        #endregion


        #region Performance enhancements
        public void Refresh()
        {
            //Most of this code is simply replicated and retooled from Kerberos.Sots.UI.FleetWidget.Refresh(),
            //  adjusted to work in a static class to duplicate functionality. While it's different, it's mostly just
            //  changes to that I can make changes externally, unobtrusively to the original code.
            //  I will note where changes in logic *actully* occur.


            //first change took the time from about 1 minute down to 8 seconds. This is still too long.


            if (this.Widget._contentChanged && this.Widget._ready)
            {
                this.Widget.PostSetProp("RefreshEnabled", false);
                this.Widget.PostSetProp("SetJumboMode", this.Widget._jumboMode);
                this.Widget.PostSetProp("EnemySelectionEnabled", this.Widget._enemySelectionEnabled);
                this.Widget.PostSetProp("SetScrapEnabled", this.Widget._scrapEnabled);
                if (this.Widget._missionMode != MissionType.NO_MISSION)
                {
                    this.Widget.SetMissionMode(true);
                }
                else
                {
                    this.Widget.SetMissionMode(false);
                }
                if (this.Widget._fleetsChanged)
                {
                    this.Widget.ClearFleets();
                    this.Widget.AccountedSystems.Clear();

                    List<FleetInfo> syncedFleets = new List<FleetInfo>();
                    foreach (int current in this.Widget._syncedFleets)
                    {
                        FleetInfo fleetInfo = this.Widget.App.GameDatabase.GetFleetInfo(current);
                        if (fleetInfo != null)
                            syncedFleets.Add(fleetInfo);
                    }
                    syncedFleets = syncedFleets.OrderBy(x => x.SystemID).ToList();

                    /****************************************************************************
                    *   This is where the logic change is. SyncFleet is looped over and over,   *
                    *   and it calls DesignLab.GetMissionRequiredDesigns over and over.         *
                    *                                                                           *
                    *   Since I don't see anything changing the mission, I believe this should  *
                    *   be safe.                                                                *
                    *                                                                           *
                    *   Furthermore, if _missionMode is neither colonization, support,          *
                    *   nor construction, then the call returns an empty list.                  *
                    *   The only time I can see this EVER being slower is if there is no        *
                    *   fleet whatsoever for the requested type.                                *
                    ****************************************************************************/
                    List<Int32> missionRequiredDesigns = PerformanceStrategy.DesignLab.GetMissionRequiredDesigns(this.Widget._game.Game, this.Widget._missionMode, this.Widget._game.LocalPlayer.ID);
                    Dictionary<Int32, List<SectionInstanceInfo>> groupedShipSections = PerformanceData.GameDatabase.GroupCachedShipSectionsByShip(this.Widget.App.GameDatabase);

                    //The command point bonuses are expensive when iterated over so repeatedly, yet returning the same result
                    Int32 cmdPtBonus = PerformanceData.GameDatabase.GetCommandPointBonus(this.Widget.App.GameDatabase, this.Widget._game.LocalPlayer.ID);


                    FleetInfo firstReserveFleet = syncedFleets.FirstOrDefault(x => x.IsReserveFleet && x.PlayerID == this.Widget.App.LocalPlayer.ID);
                    if (firstReserveFleet != null)
                    {
                        if (!this.Widget.AccountedSystems.Contains(firstReserveFleet.SystemID))
                        {
                            this.Widget.AccountedSystems.Add(firstReserveFleet.SystemID);
                            this.Widget.SyncSystem(this.Widget.App.GameDatabase.GetStarSystemInfo(firstReserveFleet.SystemID));
                        }
                        this.SyncFleet(firstReserveFleet, missionRequiredDesigns, groupedShipSections, cmdPtBonus);  //pass in the pre-loaded mission designs, ship sections and command point bonuses
                    }

                    FleetInfo firstDefenseFleet = syncedFleets.FirstOrDefault((FleetInfo x) => x.IsDefenseFleet && x.PlayerID == this.Widget.App.LocalPlayer.ID);
                    if (firstDefenseFleet != null)
                    {
                        if (!this.Widget.AccountedSystems.Contains(firstDefenseFleet.SystemID))
                        {
                            this.Widget.AccountedSystems.Add(firstDefenseFleet.SystemID);
                            this.Widget.SyncSystem(this.Widget.App.GameDatabase.GetStarSystemInfo(firstDefenseFleet.SystemID));
                        }
                        this.SyncFleet(firstDefenseFleet, missionRequiredDesigns, groupedShipSections, cmdPtBonus);  //pass in the pre-loaded mission designs, ship sections and command point bonuses
                    }

                    if (this.Widget._listStations)
                    {
                        List<StationInfo> syncedStations = new List<StationInfo>();
                        foreach (int current2 in this.Widget._syncedStations)
                        {
                            StationInfo stationInfo = this.Widget.App.GameDatabase.GetStationInfo(current2);
                            if (stationInfo != null)
                            {
                                syncedStations.Add(stationInfo);
                            }
                        }
                        this.Widget.SyncStations(syncedStations);
                    }

                    foreach (FleetInfo fleet in syncedFleets)
                    {
                        if (fleet != firstReserveFleet && (fleet.Type != FleetType.FL_RESERVE || fleet.PlayerID == this.Widget.App.LocalPlayer.ID) && fleet != firstDefenseFleet && (fleet.Type != FleetType.FL_DEFENSE || fleet.PlayerID == this.Widget.App.LocalPlayer.ID))
                        {
                            if (!this.Widget.AccountedSystems.Contains(fleet.SystemID))
                            {
                                this.Widget.AccountedSystems.Add(fleet.SystemID);
                                this.Widget.SyncSystem(this.Widget.App.GameDatabase.GetStarSystemInfo(fleet.SystemID));
                            }
                            this.SyncFleet(fleet, missionRequiredDesigns, groupedShipSections, cmdPtBonus);  //pass in the pre-loaded mission designs, ship sections and command point bonuses
                        }
                    }
                    this.Widget._fleetsChanged = false;
                }
                if (this.Widget._planetsChanged)
                {
                    this.Widget.ClearPlanets();
                    this.Widget.SyncPlanets();
                    this.Widget._planetsChanged = false;
                }
                this.Widget.PostSetProp("SetSelected", this.Widget._selected);
                this.Widget.PostSetProp("ShowFleetInfo", this.Widget._showFleetInfo);
                this.Widget.PostSetProp("ShipSelectionEnabled", this.Widget._shipSelectionEnabled);
                this.Widget.PostSetProp("SetPreferredSelectMode", this.Widget._preferredSelectMode);
                if (this.Widget._hasSuulka)
                {
                    this.Widget.App.UI.SetVisible("gameRepairSuulkasButton", true);
                }
                if (this.Widget._repairModeChanged)
                {
                    this.Widget.PostSetProp("SetRepairMode", this.Widget._repairMode);
                    this.Widget._repairModeChanged = false;
                }
                this.Widget.PostSetProp("ShowRepairPoints", this.Widget._showRepairPoints);
                this.Widget.PostSetProp("RepairWidget", this.Widget._repairWidget);
                if (this.Widget._expandAll)
                {
                    this.Widget.PostSetProp("ExpandAll", new object[0]);
                    this.Widget._expandAll = false;
                }
                this.Widget.PostSetProp("Enabled", this.Widget._enabled);
                if (this.Widget._repairAll)
                {
                    this.Widget.PostSetProp("RepairAll", new object[0]);
                    this.Widget._repairAll = false;
                }
                if (this.Widget._undoAll)
                {
                    this.Widget.PostSetProp("UndoAll", new object[0]);
                    this.Widget._undoAll = false;
                }
                if (this.Widget._confirmRepairs)
                {
                    this.Widget.PostSetProp("ConfirmRepairs", new object[0]);
                    this.Widget._confirmRepairs = false;
                }
                this.Widget._contentChanged = false;
                this.Widget.PostSetProp("RefreshEnabled", true);
                this.Widget.SetShowEmptyFleets(this.Widget._showEmptyFleets);
            }
        }

        /// <summary>Syncs a specified fleet with the database</summary>
        /// <param name="fleet">Fleet to sync</param>
        /// <param name="missionRequiredDesigns">Pre-populated list of Design IDs that fulfill a mission possibly being attempted (colonization, support, construction)</param>
        /// <param name="groupedShipSections">Pre-populated Collection of SectionInstances, already grouped by the ShipID</param>
        /// <param name="playerCommandPointBonus">Pre-computed bonus of the current player's command point bonuses from technologies researched</param>
        /// <remarks>
        ///     Pre-populating the additional parameters saves quite a bit of time computationally since this method
        ///     is called quite extensively for each fleet, and some of the calls from this are for each ship;
        ///     since the values are the same throughout all of this, it makes sense to pull it only once.
        /// </remarks>
        public void SyncFleet(FleetInfo fleet, List<Int32> missionRequiredDesigns, Dictionary<Int32, List<SectionInstanceInfo>> groupedShipSections, Int32 playerCommandPointBonus)
        {
            Func<ColonyInfo, bool> predicate = null;
            if ((fleet != null) && (((this.Widget._game.GameDatabase.GetMissionByFleetID(fleet.ID) == null) || (this.Widget._game.GameDatabase.GetMissionByFleetID(fleet.ID).Type != MissionType.PIRACY)) || (this.Widget._game.GameDatabase.PirateFleetVisibleToPlayer(fleet.ID, this.Widget._game.LocalPlayer.ID) || this.Widget.ShowPiracyFleets)))
            {
                this.Widget.SyncFleetInfo(fleet);
                if (this.Widget.SuulkaMode)
                {
                    this.Widget.SyncSuulkas(fleet);
                }
                else
                {
                    List<ShipInfo> list = this.Widget._game.GameDatabase.GetShipInfoByFleetID(fleet.ID, false).ToList<ShipInfo>();
                    List<object> list2 = new List<object>
                    {
                        fleet.Type,
                        fleet.ID
                    };
                    int count = list2.Count;
                    int item = 0;
                    List<int> list3 = null;
                    if ((this.Widget._missionMode != MissionType.NO_MISSION) && (fleet.Type != FleetType.FL_RESERVE))
                    {
                        list3 = Kerberos.Sots.StarFleet.StarFleet.GetMissionCapableShips(this.Widget._game.Game, fleet.ID, this.Widget._missionMode);
                        if (list3.Count == 0)
                        {
                            /********************************************************
                            *   Removed the missionRequiredDesigns collection here  *
                            ********************************************************/

                            foreach (int num3 in missionRequiredDesigns)
                            {
                                item++;
                                list2.Add(true);
                                DesignInfo designInfo = this.Widget.App.GameDatabase.GetDesignInfo(num3);
                                list2.Add(num3);
                                list2.Add(-1);
                                list2.Add(designInfo.Name);
                                list2.Add(designInfo.Name);
                                list2.Add(false);
                                list2.Add(false);
                                list2.Add("");
                                list2.Add("");
                                list2.Add(0);
                                list2.Add(false);
                                list2.Add(false);
                                list2.Add(this.Widget.App.GameDatabase.GetCommandPointCost(designInfo.ID));
                                list2.Add(PerformanceData.GameDatabase.GetDesignCommandPointQuota(this.Widget.App.GameDatabase, designInfo.ID, playerCommandPointBonus));
                                list2.Add(true);
                                list2.Add(0);
                                list2.Add(0);
                                list2.Add(0);
                                list2.Add(0);
                                list2.Add(0);
                                list2.Add(0);
                                list2.Add(0);
                                list2.Add(0);
                                list2.Add(designInfo.Class);
                                list2.Add(designInfo.DesignSections.First<DesignSectionInfo>(x => (x.ShipSectionAsset.Type == ShipSectionType.Mission)).ShipSectionAsset.BattleRiderType);
                            }
                        }
                    }
                    foreach (ShipInfo info2 in list)
                    {
                        DesignInfo design = this.Widget.App.GameDatabase.GetDesignInfo(info2.DesignID);
                        bool flag = true;
                        if (this.Widget.ShipFilter != null)
                        {
                            //HACK: .NET does not offer the best event/delegate reflection handling offerings, so this looks really nasty
                            switch ((Original.FleetWidget.FilterShips)this.Widget.ShipFilter.DynamicInvoke(new Object[] { info2, design }))
                            {
                                case Original.FleetWidget.FilterShips.Ignore:
                                    continue;
                                case Original.FleetWidget.FilterShips.Disable:
                                    flag = false;
                                    break;
                            }
                        }
                        item++;
                        if ((list3 != null) && (this.Widget._missionMode != MissionType.NO_MISSION))
                        {
                            list2.Add(list3.Contains(info2.ID));
                        }
                        else
                        {
                            list2.Add(true);
                        }
                        list2.Add(info2.DesignID);
                        list2.Add(info2.ID);
                        list2.Add(design.Name);
                        list2.Add(info2.ShipName);
                        bool flag2 = false;
                        string iconSpriteName = "";
                        string str2 = "";
                        bool flag3 = false;
                        bool flag4 = info2.IsPoliceShip();
                        bool flag5 = false;
                        foreach (DesignSectionInfo info4 in design.DesignSections)
                        {
                            if (info4.ShipSectionAsset.GetPlatformType().HasValue)
                            {
                                str2 = info4.ShipSectionAsset.GetPlatformType().ToString();
                            }
                            if (info4.FilePath.Contains("minelayer"))
                            {
                                flag2 = true;
                                foreach (WeaponBankInfo info5 in info4.WeaponBanks)
                                {
                                    Func<LogicalWeapon, bool> func = null;
                                    string wasset = this.Widget.App.GameDatabase.GetWeaponAsset(info5.WeaponID.Value);
                                    if (wasset.Contains("Min_"))
                                    {
                                        if (func == null)
                                        {
                                            func = x => x.FileName == wasset;
                                        }
                                        LogicalWeapon weapon = this.Widget.App.AssetDatabase.Weapons.FirstOrDefault<LogicalWeapon>(func);
                                        if (weapon != null)
                                        {
                                            iconSpriteName = weapon.IconSpriteName;
                                            break;
                                        }
                                    }
                                }
                                break;
                            }
                            if (info4.FilePath.ToLower().Contains("_sdb"))
                            {
                                flag3 = true;
                                break;
                            }
                            if (info4.ShipSectionAsset.isPolice)
                            {
                                flag4 = true;
                                break;
                            }
                            if (info4.ShipSectionAsset.IsSuperTransport)
                            {
                                flag5 = true;
                                break;
                            }
                        }
                        list2.Add(flag2);
                        list2.Add(flag3);
                        list2.Add(iconSpriteName);
                        list2.Add(str2);
                        list2.Add(info2.LoaCubes);
                        list2.Add(flag5);
                        list2.Add(flag4);
                        list2.Add(this.Widget.App.GameDatabase.GetShipCommandPointCost(info2.ID, true));
                        list2.Add(PerformanceData.GameDatabase.GetDesignCommandPointQuota(this.Widget.App.GameDatabase, design.ID, playerCommandPointBonus));
                        list2.Add(flag);
                        int num4 = 0;
                        int num5 = 0;
                        int num6 = 0;
                        int num7 = 0;
                        int num8 = 0;
                        int num9 = 0;
                        int num10 = 0;
                        int num11 = 0;
                        int num12 = 0;
                        BattleRiderTypes unspecified = BattleRiderTypes.Unspecified;

                        List<SectionInstanceInfo> list5 = groupedShipSections[info2.ID];
                        for (int i = 0; i < design.DesignSections.Count<DesignSectionInfo>(); i++)
                        {
                            ShipSectionAsset shipSectionAsset = this.Widget.App.AssetDatabase.GetShipSectionAsset(design.DesignSections[i].FilePath);
                            List<string> techs = new List<string>();
                            if (design.DesignSections[i].Techs.Count > 0)
                            {
                                foreach (int num14 in design.DesignSections[i].Techs)
                                {
                                    techs.Add(this.Widget.App.GameDatabase.GetTechFileID(num14));
                                }
                            }
                            float structure = list5[i].Structure;
                            num7 += Ship.GetStructureWithTech(this.Widget._game.AssetDatabase, techs, shipSectionAsset.Structure);
                            num6 += list5[i].Structure;
                            num8 += shipSectionAsset.ConstructionPoints;
                            num9 += shipSectionAsset.ColonizationSpace;
                            if (shipSectionAsset.Type == ShipSectionType.Mission)
                            {
                                unspecified = shipSectionAsset.BattleRiderType;
                            }
                            Dictionary<ArmorSide, DamagePattern> armorInstances = this.Widget.App.GameDatabase.GetArmorInstances(list5[i].ID);
                            if (armorInstances.Count > 0)
                            {
                                for (int j = 0; j < 4; j++)
                                {
                                    DamagePattern dp = armorInstances[(ArmorSide)j];
                                    num7 += (dp.Width * dp.Height) * 3;
                                    for (int k = 0; k < dp.Width; k++)
                                    {
                                        for (int m = 0; m < dp.Height; m++)
                                        {
                                            if (!dp.GetValue(k, m))
                                            {
                                                num6 += 3;
                                            }
                                        }
                                    }
                                }
                            }
                            Func<WeaponBankInfo, bool> func2 = null;
                            foreach (LogicalMount mount in shipSectionAsset.Mounts)
                            {
                                if (WeaponEnums.IsWeaponBattleRider(mount.Bank.TurretClass))
                                {
                                    if (num12 == 0)
                                    {
                                        if (func2 == null)
                                        {
                                            func2 = delegate(WeaponBankInfo x)
                                            {
                                                if (!(x.BankGUID == mount.Bank.GUID) || !x.DesignID.HasValue)
                                                {
                                                    return false;
                                                }
                                                int? designID = x.DesignID;
                                                if (designID.GetValueOrDefault() == 0)
                                                {
                                                    return !designID.HasValue;
                                                }
                                                return true;
                                            };
                                        }
                                        WeaponBankInfo info6 = design.DesignSections[i].WeaponBanks.FirstOrDefault<WeaponBankInfo>(func2);
                                        num12 = ((info6 != null) && info6.DesignID.HasValue) ? info6.DesignID.Value : 0;
                                    }
                                    num10++;
                                }
                            }
                            List<ModuleInstanceInfo> list7 = this.Widget.App.GameDatabase.GetModuleInstances(list5[i].ID).ToList<ModuleInstanceInfo>();
                            List<DesignModuleInfo> module = design.DesignSections[i].Modules;
                            if (list7.Count == module.Count)
                            {
                                Func<ModuleInstanceInfo, bool> func3 = null;
                                for (int mod = 0; mod < module.Count; mod++)
                                {
                                    string modAsset;
                                    if (func3 == null)
                                    {
                                        func3 = x => x.ModuleNodeID == module[mod].MountNodeName;
                                    }
                                    ModuleInstanceInfo info7 = list7.FirstOrDefault<ModuleInstanceInfo>(func3);
                                    if (info7 != null)
                                    {
                                        modAsset = this.Widget.App.GameDatabase.GetModuleAsset(module[mod].ModuleID);
                                        LogicalModule logicalModule = (from x in this.Widget.App.AssetDatabase.Modules
                                                                       where x.ModulePath == modAsset
                                                                       select x).First<LogicalModule>();
                                        num7 += (int)logicalModule.Structure;
                                        num6 += (info7 != null) ? info7.Structure : ((int)logicalModule.Structure);
                                        num5 += logicalModule.RepairPointsBonus;
                                        if (info7.Structure > 0f)
                                        {
                                            num4 += info7.RepairPoints;
                                            structure += logicalModule.StructureBonus;
                                        }
                                        if (module[mod].DesignID.HasValue)
                                        {
                                            foreach (LogicalMount mount in logicalModule.Mounts)
                                            {
                                                if (WeaponEnums.IsWeaponBattleRider(mount.Bank.TurretClass))
                                                {
                                                    if (num12 == 0)
                                                    {
                                                        num12 = module[mod].DesignID.Value;
                                                    }
                                                    num10++;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            num5 += shipSectionAsset.RepairPoints;
                            if (structure > 0f)
                            {
                                num4 += list5[i].RepairPoints;
                            }
                            foreach (WeaponInstanceInfo info8 in this.Widget.App.GameDatabase.GetWeaponInstances(list5[i].ID).ToList<WeaponInstanceInfo>())
                            {
                                num7 += (int)info8.MaxStructure;
                                num6 += (int)info8.Structure;
                            }
                        }
                        List<ShipInfo> source = this.Widget.App.GameDatabase.GetBattleRidersByParentID(info2.ID).ToList<ShipInfo>();
                        if (num10 > 0)
                        {
                            num11 = num10;
                            foreach (ShipInfo info9 in source)
                            {
                                DesignInfo info10 = this.Widget.App.GameDatabase.GetDesignInfo(info9.DesignID);
                                if (info10 != null)
                                {
                                    DesignSectionInfo info11 = info10.DesignSections.FirstOrDefault<DesignSectionInfo>(x => x.ShipSectionAsset.Type == ShipSectionType.Mission);
                                    if ((info11 != null) && ShipSectionAsset.IsBattleRiderClass(info11.ShipSectionAsset.RealClass))
                                    {
                                        num11--;
                                    }
                                }
                            }
                            int num19 = 0;
                            int repairPoints = 0;
                            if (num12 != 0)
                            {
                                foreach (DesignSectionInfo info13 in this.Widget.App.GameDatabase.GetDesignInfo(num12).DesignSections)
                                {
                                    ShipSectionAsset asset2 = this.Widget.App.AssetDatabase.GetShipSectionAsset(info13.FilePath);
                                    num19 = asset2.Structure;
                                    repairPoints = asset2.RepairPoints;
                                    if (asset2.Armor.Length > 0)
                                    {
                                        for (int n = 0; n < 4; n++)
                                        {
                                            num19 += (asset2.Armor[n].X * asset2.Armor[n].Y) * 3;
                                        }
                                    }
                                }
                            }
                            num7 += num19 * num10;
                            num5 += repairPoints * num10;
                            num6 += num19 * (num10 - num11);
                            num4 += repairPoints * (num10 - num11);
                        }
                        if (list5.Count != design.DesignSections.Length)
                        {
                            throw new InvalidDataException(string.Format("Mismatched design section vs ship section instance count for designId={0} and shipId={1}.", design.ID, info2.ID));
                        }
                        list2.Add(num6);
                        list2.Add(num7);
                        list2.Add(num4);
                        list2.Add(num5);
                        list2.Add(num8);
                        list2.Add(num9);
                        list2.Add(source.Count<ShipInfo>());
                        foreach (ShipInfo info14 in source)
                        {
                            list2.Add(info14.ID);
                        }
                        list2.Add(0);
                        list2.Add(design.Class);
                        list2.Add((int)unspecified);
                    }
                    list2.Insert(count, item);
                    bool flag6 = fleet.Type == FleetType.FL_RESERVE;
                    count = list2.Count;
                    int num22 = 0;
                    if (flag6 && this.Widget._showColonies)
                    {
                        List<ColonyInfo> list10;
                        if (this.Widget._onlyLocalPlayer)
                        {
                            if (predicate == null)
                            {
                                predicate = x => x.PlayerID == this.Widget.App.LocalPlayer.ID;
                            }
                            list10 = this.Widget.App.GameDatabase.GetColonyInfosForSystem(fleet.SystemID).ToList<ColonyInfo>().Where<ColonyInfo>(predicate).ToList<ColonyInfo>();
                        }
                        else
                        {
                            list10 = this.Widget.App.GameDatabase.GetColonyInfosForSystem(fleet.SystemID).ToList<ColonyInfo>();
                        }
                        foreach (ColonyInfo info15 in list10)
                        {
                            list2.Add(info15.ID);
                            list2.Add(this.Widget.App.GameDatabase.GetOrbitalObjectInfo(info15.OrbitalObjectID).Name);
                            list2.Add(info15.RepairPoints);
                            list2.Add(info15.RepairPointsMax);
                            num22++;
                        }
                    }
                    list2.Insert(count, num22);
                    this.Widget.PostSetProp("SyncShips", list2.ToArray());
                }
            }
        }
        #endregion
	}
}