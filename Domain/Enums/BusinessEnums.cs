namespace TeknoSOS.WebApp.Domain.Enums
{
    /// <summary>
    /// Categories for businesses in the TeknoSOS ecosystem
    /// </summary>
    public enum BusinessCategory
    {
        // Materials & Supplies
        ElectricalSupplies = 1,      // Materiale elektrike
        PlumbingSupplies = 2,        // Materiale hidraulike
        ConstructionMaterials = 3,   // Materiale ndërtimi
        HVACSupplies = 4,            // Materiale ngrohje/ftohje
        PaintAndFinishes = 5,        // Bojë dhe perfundime
        ToolsAndEquipment = 6,       // Vegla dhe pajisje
        SafetyEquipment = 7,         // Pajisje sigurie
        LightingSupplies = 8,        // Materiale ndriçimi
        
        // Professional Services
        ArchitecturalServices = 20,  // Shërbime arkitekture
        EngineeringServices = 21,    // Shërbime inxhinierike
        InteriorDesign = 22,         // Dizajn enterieri
        PermitServices = 23,         // Shërbime leje (bashkia)
        InspectionServices = 24,     // Shërbime inspektimi
        
        // Rental Services
        EquipmentRental = 40,        // Qiradhënie pajisjesh
        ScaffoldingRental = 41,      // Qiradhënie skelash
        VehicleRental = 42,          // Qiradhënie automjetesh
        
        // Support Services
        WasteDisposal = 60,          // Largim mbeturinash
        Logistics = 61,              // Logjistikë/transport
        Cleaning = 62,               // Pastrim
        Security = 63,               // Siguri fizike
        
        // Other
        GeneralContractor = 80,      // Kontraktor i përgjithshëm
        Other = 99                   // Të tjera
    }

    /// <summary>
    /// Type of business entity
    /// </summary>
    public enum BusinessType
    {
        Supplier = 1,        // Furnizues materialesh
        ServiceProvider = 2, // Ofrues shërbimesh
        Both = 3             // Të dyja
    }

    /// <summary>
    /// Business verification status
    /// </summary>
    public enum BusinessVerificationStatus
    {
        Pending = 0,         // Në pritje verifikimi
        Verified = 1,        // I verifikuar
        Rejected = 2,        // I refuzuar
        Suspended = 3        // I pezulluar
    }
}
