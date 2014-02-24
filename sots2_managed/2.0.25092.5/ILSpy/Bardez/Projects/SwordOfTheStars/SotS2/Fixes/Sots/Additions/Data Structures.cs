using System;
using System.Collections.Generic;
using System.Linq;

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.ShipFramework;

/************************************************************************************
*   This file contains new data structures used in performance enhancement calls    *
*   These will mostly be used to wrap parameters to method calls.                   *
*   I could use Tuples instead of these, but in almost every single where I have    *
*   I find reason not to and later refactor                                         *
************************************************************************************/

namespace Bardez.Projects.SwordOfTheStars.SotS2.Fixes.Sots.Additions
{
    /// <summary>Class that contains a ship insertion's parameters and return value</summary>
    internal class ShipInsertionParameters
    {
        #region Fields
        /// <summary>Unique ID of the fleet to add a ship to</summary>
        public Int32 FleetID { get; set; }

        /// <summary>Unique ID of the design of the ship to add</summary>
        public Int32 DesignID { get; set; }

        /// <summary>Name of the design</summary>
        public String DesignName { get; set; }

        /// <summary>Various ship parameters to set (such as whether it is a deployed gate)</summary>
        public ShipParams ShipFlags { get; set; }

        /// <summary>Unique ID of the AIFleet for this ship, if applicable</summary>
        public Int32? AIFleetID { get; set; }

        /// <summary>The number of Loa Cubes used(?) to create this ship</summary>
        public Int32 LoaCubes { get; set; }

        /// <summary>Parent Ship that was inserted; used for assigning battle riders</summary>
        public ShipInsertionParameters ParentShip { get; set; }

        /// <summary>Slot Index for a battle rider; -1 is N/A</summary>
        public Int32 SlotIndex { get; set; }

        /// <summary>Create ShipInfo of the inserted ship (return value)</summary>
        public ShipInfo ShipInfo { get; set; }
        #endregion


        #region Construction
        /// <summary>Parameter constructor</summary>
        /// <param name="fleetId">Unique ID of the fleet to add a ship to</param>
        /// <param name="designId">Unique ID of the design of the ship to add</param>
        /// <param name="designName">Name of the design</param>
        /// <param name="flags">Various ship parameters to set (such as whether it is a deployed gate)</param>
        /// <param name="aiFleetId">Unique ID of the AIFleet for this ship, if applicable</param>
        /// <param name="loaCubes">The number of Loa Cubes used(?) to create this ship</param>
        /// <param name="parentShip">Parent Ship that was inserted; used for assigning battle riders</param>
        /// <param name="slotIndex">Slot Index for a battle rider; -1 is N/A</param>
        public ShipInsertionParameters(Int32 fleetId, Int32 designId, String designName, ShipParams flags, Int32? aiFleetId, Int32 loaCubes, ShipInsertionParameters parentShip, Int32 slotIndex)
        {
            this.FleetID = fleetId;
            this.DesignID = designId;
            this.DesignName = designName;
            this.ShipFlags = flags;
            this.AIFleetID = aiFleetId;
            this.LoaCubes = loaCubes;
            this.ParentShip = parentShip;
            this.SlotIndex = slotIndex;
        }
        #endregion
    }

    /// <summary>Class that contains Ship section insertion parameters</summary>
    internal class ShipSectionInsertionParameters
    {
        #region Fields
        /// <summary>Design of the ship having sections inserted</summary>
        public DesignInfo DesignInfo { get ;set; }

        /// <summary>Reference to the ship info being inserted, to retrieve Ship ID once inserted</summary>
        public ShipInfo ShipInfo { get; set; }

        /// <summary>Unique ID of the station that the section is being inserted for, if applicable</summary>
        public Int32? StationID { get; set; }
        #endregion


        #region Construction
        /// <summary>Definition constructor</summary>
        /// <param name="design">Design of the ship having sections inserted</param>
        /// <param name="ship">Reference to the ship info being inserted, to retrieve Ship ID once inserted</param>
        /// <param name="stationId">Unique ID of the station that the section is being inserted for, if applicable</param>
        public ShipSectionInsertionParameters(DesignInfo design, ShipInfo ship, Int32? stationId)
        {
            this.DesignInfo = design;
            this.ShipInfo = ship;
            this.StationID = stationId;
        }
        #endregion
    }
    
    /// <summary>Class that contains ship section armor insertion parameters</summary>
    internal class ArmorInstanceInsertionParameters
    {
        #region Fields
        /// <summary>Asset file definition for ship section</summary>
        public ShipSectionAsset ShipSectionAsset { get; set; }

        /// <summary>Attribute associated with design</summary>
        public IEnumerable<SectionEnumerations.DesignAttribute> Attributes { get; set; }

        /// <summary>ShipSection that is to be inserted, to reference its section ID</summary>
        public SectionInstanceInfo ShipSection { get; set; }
        #endregion


        #region Construction
        /// <summary>Definition constructor</summary>
        /// <param name="asset">Asset file definition for ship section</param>
        /// <param name="attributes">Attribute associated with design</param>
        /// <param name="section">ShipSection that is to be inserted, to reference its section ID</param>
        public ArmorInstanceInsertionParameters(ShipSectionAsset asset, IEnumerable<SectionEnumerations.DesignAttribute> attributes, SectionInstanceInfo section)
        {
            this.ShipSectionAsset = asset;
            this.Attributes = attributes;
            this.ShipSection = section;
        }
        #endregion
    }
    
    /// <summary>Class that contains ship section armor insertion values</summary>
    internal class ArmorInstanceInsertion
    {
        #region Fields
        /// <summary>Armor to insert</summary>
        public Dictionary<ArmorSide, DamagePattern> Armor { get; set; }

        /// <summary>ShipSection that is to be inserted, to reference its section ID</summary>
        public SectionInstanceInfo ShipSection { get; set; }
        #endregion


        #region Construction
        /// <summary>Definition constructor</summary>
        /// <param name="armor">Armor to insert</param>
        /// <param name="section">ShipSection that is to be inserted, to reference its section ID</param>
        public ArmorInstanceInsertion(Dictionary<ArmorSide, DamagePattern> armor, SectionInstanceInfo section)
        {
            this.Armor = armor;
            this.ShipSection = section;
        }
        #endregion
    }

    /// <summary>Class that contains ship section module insertion parameters</summary>
    internal class ShipModuleInstanceInsertionParameters
    {
        #region Fields
        /// <summary>Ship section asset to reference</summary>
        public ShipSectionAsset SectionAsset { get; set; }
        
        /// <summary>Collection of modules to insert instances of</summary>
        public IList<DesignModuleInfo> Modules { get; set; }

        /// <summary>ShipSection that is to be inserted, to reference its section ID</summary>
        public SectionInstanceInfo ShipSection { get; set; }
        #endregion


        #region Construction
        /// <summary>Definition constructor</summary>
        /// <param name="asset">Ship section asset to reference</param>
        /// <param name="modules">Collection of modules to insert instances of</param>
        /// <param name="section">ShipSection that is to be inserted, to reference its section ID</param>
        public ShipModuleInstanceInsertionParameters(ShipSectionAsset asset, IList<DesignModuleInfo> modules, SectionInstanceInfo section)
        {
            this.SectionAsset = asset;
            this.Modules = modules;
            this.ShipSection = section;
        }
        #endregion
    }
    
    /// <summary>Class that contains ship section weapon insertion parameters</summary>
    internal class SectionWeaponInstanceInsertionParameters
    {
        #region Fields
        /// <summary>Collection of weapon mounts to insert weapons for</summary>
        public IList<LogicalMount> Mounts { get; set;}

        /// <summary>Collection of weapon banks to insert</summary>
        public IList<WeaponBankInfo> Banks { get; set; }

        /// <summary>ShipSection that is to be inserted, to reference its section ID</summary>
        public SectionInstanceInfo ShipSection { get; set; }
        #endregion


        #region Construction
        /// <summary>Definition constructor</summary>
        /// <param name="mounts">Collection of weapon mounts to insert weapons for</param>
        /// <param name="weapons">Collection of weapon banks to insert</param>
        /// <param name="section">ShipSection that is to be inserted, to reference its section ID</param>
        public SectionWeaponInstanceInsertionParameters(IList<LogicalMount> mounts, IList<WeaponBankInfo> weapons, SectionInstanceInfo section)
        {
            this.Mounts = mounts;
            this.Banks = weapons;
            this.ShipSection = section;
        }
        #endregion
    }

    /// <summary>Class that contains ship section weapon insertion parameters</summary>
    internal class ModuleWeaponInstanceInsertionParameters
    {
        #region Fields
        /// <summary>Collection of weapon mounts to insert weapons for</summary>
        public IList<LogicalMount> Mounts { get; set; }

        /// <summary>Unique ID of the weapon to be added</summary>
        public Int32? WeaponID { get; set; }

        /// <summary>ShipSection that is to be inserted, to reference its section ID</summary>
        public SectionInstanceInfo ShipSection { get; set; }

        /// <summary>Module for which to add the weapon</summary>
        public ModuleInstanceInfo ModuleInstance { get; set; }
        #endregion


        #region Construction
        /// <summary>Definition constructor</summary>
        /// <param name="mounts">Collection of weapon mounts to insert weapons for</param>
        /// <param name="weapon">Unique ID of the weapon to be added</param>
        /// <param name="section">ShipSection that is to be inserted, to reference its section ID</param>
        /// <param name="module">Module for which to add the weapon</param>
        public ModuleWeaponInstanceInsertionParameters(IList<LogicalMount> mounts, Int32? weapon, SectionInstanceInfo section, ModuleInstanceInfo module)
        {
            this.Mounts = mounts;
            this.WeaponID = weapon;
            this.ShipSection = section;
            this.ModuleInstance = module;
        }
        #endregion
    }
}