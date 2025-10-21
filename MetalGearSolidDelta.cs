using System.Diagnostics.CodeAnalysis;
using System.Text;
using ConnectorLib.Memory;
using CrowdControl.Common;
using AddressChain = ConnectorLib.Memory.AddressChain<ConnectorLib.Inject.InjectConnector>;
using ConnectorType = CrowdControl.Common.ConnectorType;
using Log = CrowdControl.Common.Log;

namespace CrowdControl.Games.Packs.MetalGearSolidDelta;

[SuppressMessage("ReSharper", "StringLiteralTypo")]
public class MetalGearSolidDelta : InjectEffectPack
{
    public override Game Game { get; } = new("Metal Gear Solid Delta: Snake Eater", "MetalGearSolidDelta", "PC", ConnectorType.PCConnector);

    public MetalGearSolidDelta(UserRecord player, Func<CrowdControlBlock, bool> responseHandler, Action<object> statusUpdateHandler)
        : base(player, responseHandler, statusUpdateHandler)
    {
        VersionProfiles = [new("MGSDelta-Win64-Shipping", InitGame, DeinitGame)];
    }

    #region AddressChains

    // Weapon Ammo and Item Base Addresses points to NONE Weapon and NONE Item
    private AddressChain baseWeaponAddress;
    private AddressChain baseItemAddress;


    // Snake's Stats
    private AddressChain snakeHealth;
    private AddressChain snakeMaxHealth;
    private AddressChain snakeStamina;
    private AddressChain snakeCurrentEquippedWeapon;
    private AddressChain snakeCurrentEquippedItem;
    private AddressChain snakeCurrentCamo;
    private AddressChain snakeCurrentFacePaint;
    private AddressChain snakeTrips;
    private AddressChain snakeSleeps;
    private AddressChain snakePukeFire;
    private AddressChain snakeCommonCold;
    private AddressChain snakePoison;
    private AddressChain snakeFoodPoisoning;
    private AddressChain snakeHasLeeches;
    private AddressChain snakeDamageMultiplierInstructions;
    private AddressChain snakeDamageMultiplierValue;
    private AddressChain camoIndexInstructions;
    private AddressChain camoIndexInstructions2;
    private AddressChain camoIndexValue;
    private AddressChain snakeTacticalReloadInstructions;
    private AddressChain snakeManualReloadInstructions;
    private AddressChain everyoneIsDrunk;

    // Game State
    private AddressChain isPausedOrMenu;
    private AddressChain alertStatus;
    private AddressChain alertTimer;
    private AddressChain evasionTimer;
    private AddressChain cautionTimer;
    private AddressChain mapStringAddress;
    private AddressChain restartStage;

    // Filters
    private AddressChain filterInstructions;
    private AddressChain filterR;
    private AddressChain filterG;
    private AddressChain filterB;
    private AddressChain filterA;
    private AddressChain lightColourInstructions;
    private AddressChain lightColourR;
    private AddressChain lightColourG;
    private AddressChain lightColourB;
    private AddressChain lightColourA;
    private AddressChain fogFilter;

    // Guard Health, Sleep & Stun Statues
    // Lethal Damage
    private AddressChain flameDamage;
    private AddressChain snakeWeaponDamage;
    private AddressChain snakeWeaponDamageMulti;
    private AddressChain guardThroatSlitDamage;

    // Sleep Damage
    private AddressChain sleepTimer1;
    private AddressChain sleepTimer3;
    private AddressChain sleepTimer2;
    private AddressChain sleepDrain;
    private AddressChain tranqHead;
    private AddressChain tranqBody;

    // Stun Damage
    private AddressChain stunTimer1;
    private AddressChain stunTimer2;
    private AddressChain stunTimer3;
    private AddressChain stunPunch;
    private AddressChain stunGrenade;

    #endregion

    #region [De]init

    private void InitGame()
    {

        Connector.PointerFormat = PointerFormat.Absolute64LE;

        baseWeaponAddress = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+135C22A0"); // Delta uses 88 bytes between weapons
        baseItemAddress = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+135C4FF4"); // 80 Bytes between items like MC

        // Snake Animations to test
        snakeTrips = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+135CC5FB");
        snakePukeFire = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+135CC5FC");
        snakeSleeps = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+135D9BDA");

        // Snake Stats
        snakeHealth = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+C53D038=>+7B4");
        snakeMaxHealth = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+C53D038=>+7B6");
        snakeStamina = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+C53D038=>+B7A");
        snakeCommonCold = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+C53D038=>+81A");
        snakePoison = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+C53D038=>+828");
        snakeFoodPoisoning = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+C53D038=>+836");
        snakeHasLeeches = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+C53D038=>+844");
        snakeCurrentEquippedWeapon = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+C53D038=>+704");
        snakeCurrentEquippedItem = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+C53D038=>+706");
        // Probably not gonna use Facepaint and Camo unless I can figure out a way to force pause the game and refresh Snake's clothes
        snakeCurrentCamo = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+C53D038=>+7AE");
        snakeCurrentFacePaint = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+C53D038=>+7AF");
        snakeDamageMultiplierInstructions = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+7ADAAD1");
        snakeDamageMultiplierValue = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+7ADAAD3");
        snakeTacticalReloadInstructions = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+7A86846");
        snakeManualReloadInstructions = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+7A871A8");
        everyoneIsDrunk = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+56F7042");

        camoIndexInstructions = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+7D1689C"); // 8B 05 36 FF FF FF | Restore: 8B 05 82 9D 8B 0B
        camoIndexInstructions2 = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+7ABFE65 "); // 8B 05 5D 40 25 00 | Restore: 8B 05 A9 DE B0 0B
        camoIndexValue = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+7D167D8"); // -1000 for -100% camo 1000 for 100% camo 4 byte value

        // Game State
        isPausedOrMenu = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+135DD8EC");
        mapStringAddress = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+C53D038=>+24");
        restartStage = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+1452D620");

        // 16 = Alert, 32 = Caution, 0 = No Alert
        alertStatus = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+135CA6C8");
        alertTimer = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+135CA674");
        evasionTimer = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+135CA68C");
        cautionTimer = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+135CA678");

        // Filters
        filterInstructions = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+2BA303D");
        filterR = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+144838F0");
        filterG = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+144838F4");
        filterB = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+144838F8");
        filterA = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+144838FC");

        lightColourInstructions = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+2BA30A8");
        lightColourR = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+14483900");
        lightColourG = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+14483904");
        lightColourB = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+14483908");
        lightColourA = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+1448390C");

        fogFilter = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+267C09F");

        // Guard Health, Sleep & Stun Statues
        // Lethal Damage
        snakeWeaponDamage = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+7CBCFDA");
        snakeWeaponDamageMulti = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+7CBCFF1");
        flameDamage = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+7CBFE36");
        guardThroatSlitDamage = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+7D210AA");

        // Sleep Damage
        sleepTimer1 = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+7CC06AB");
        sleepTimer2 = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+7CC06BD");
        sleepTimer3 = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+7CC06C9");
        sleepDrain = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+7CC1CAF");
        tranqHead = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+7CD8F64");
        tranqBody = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+7CDBAAB");

        // Stun Damage
        stunTimer1 = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+7CBFDBB");
        stunTimer2 = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+7CBFDCD");
        stunTimer3 = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+7CBFDD9");
        stunPunch = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+7CD8DC8");
        stunGrenade = AddressChain.Parse(Connector, "\"MGSDelta-Win64-Shipping.exe\"+7CD8706");

    }

    private void DeinitGame()
    {
    }

    #endregion

    #region Weapon and Items Class

    public abstract class GameObject
    {
        public string Name { get; set; }
    }

    public abstract class WeaponItemManager : GameObject
    {
        public int Index { get; set; }
        public bool HasAmmo { get; set; }
        public bool HasClip { get; set; }
        public bool HasSuppressor { get; set; }

        protected WeaponItemManager(string name, int index)
        {
            Name = name;
            Index = index;
        }
    }

    public class Weapon : WeaponItemManager
    {
        public Weapon(string name, int index, bool hasAmmo = false, bool hasClip = false, bool hasSuppressor = false)
            : base(name, index)
        {
            HasAmmo = hasAmmo;
            HasClip = hasClip;
            HasSuppressor = hasSuppressor;
        }

        public AddressChain GetPropertyAddress(AddressChain baseWeaponAddress, int propertyOffset)
        {
            int totalOffset = (WeaponAddresses.WeaponOffset * Index) + propertyOffset;
            return baseWeaponAddress.Offset(totalOffset);
        }
    }

    public class Item : WeaponItemManager
    {
        public Item(string name, int index)
            : base(name, index)
        {
        }

        public AddressChain GetPropertyAddress(AddressChain baseItemAddress, int propertyOffset)
        {
            int totalOffset = (ItemAddresses.ItemOffset * Index) + propertyOffset;
            return baseItemAddress.Offset(totalOffset);
        }
    }

    public static class WeaponAddresses
    {
        public const int WeaponOffset = 0x58;
        public const int CurrentAmmoOffset = 0x0;
        public const int MaxAmmoOffset = 0x2;
        public const int ClipOffset = 0x4;
        public const int MaxClipOffset = 0x6;
        public const int SuppressorToggleOffset = 0x10;
    }

    public static class ItemAddresses
    {
        public const int ItemOffset = 0x50;
        public const int CurrentCapacityOffset = 0x0;
        public const int MaxCapacityOffset = 0x2;
    }

    public static class MetalGearSolidDeltaUsableObjects
    {
        public static readonly Weapon NoneWeapon = new Weapon("None Weapon", 0);
        public static readonly Weapon SurvivalKnife = new Weapon("Survival Knife", 1);
        public static readonly Weapon Fork = new Weapon("Fork", 2);
        public static readonly Weapon CigSpray = new Weapon("Cig Spray", 3, true);
        public static readonly Weapon Handkerchief = new Weapon("Handkerchief", 4, true);
        public static readonly Weapon MK22 = new Weapon("MK22", 5, true, true, true);
        public static readonly Weapon M1911A1 = new Weapon("M1911A1", 6, true, true, true);
        public static readonly Weapon EzGun = new Weapon("EZ Gun", 7);
        public static readonly Weapon SAA = new Weapon("SAA", 8, true, true);
        public static readonly Weapon Patriot = new Weapon("Patriot", 9);
        public static readonly Weapon Scorpion = new Weapon("Scorpion", 10, true, true);
        public static readonly Weapon XM16E1 = new Weapon("XM16E1", 11, true, true, true);
        public static readonly Weapon AK47 = new Weapon("AK47", 12, true, true);
        public static readonly Weapon M63 = new Weapon("M63", 13, true, true);
        public static readonly Weapon M37 = new Weapon("M37", 14, true, true);
        public static readonly Weapon SVD = new Weapon("SVD", 15, true, true);
        public static readonly Weapon MosinNagant = new Weapon("Mosin-Nagant", 16, true, true);
        public static readonly Weapon RPG7 = new Weapon("RPG-7", 17, true, true);
        public static readonly Weapon Torch = new Weapon("Torch", 18);
        public static readonly Weapon Grenade = new Weapon("Grenade", 19, true);
        public static readonly Weapon WpGrenade = new Weapon("WP Grenade", 20, true);
        public static readonly Weapon StunGrenade = new Weapon("Stun Grenade", 21, true);
        public static readonly Weapon ChaffGrenade = new Weapon("Chaff Grenade", 22, true);
        public static readonly Weapon SmokeGrenade = new Weapon("Smoke Grenade", 23, true);
        public static readonly Weapon EmptyMagazine = new Weapon("Empty Magazine", 24, true);
        public static readonly Weapon TNT = new Weapon("TNT", 25, true);
        public static readonly Weapon C3 = new Weapon("C3", 26, true);
        public static readonly Weapon Claymore = new Weapon("Claymore", 27, true);
        public static readonly Weapon Book = new Weapon("Book", 28, true);
        public static readonly Weapon Mousetrap = new Weapon("Mousetrap", 29, true);
        public static readonly Weapon DirectionalMic = new Weapon("Directional Microphone", 30);

        public static readonly Dictionary<int, Weapon> AllWeapons = new Dictionary<int, Weapon>
        {
            { NoneWeapon.Index, NoneWeapon },
            { SurvivalKnife.Index, SurvivalKnife },
            { Fork.Index, Fork },
            { CigSpray.Index, CigSpray },
            { Handkerchief.Index, Handkerchief },
            { MK22.Index, MK22 },
            { M1911A1.Index, M1911A1 },
            { EzGun.Index, EzGun },
            { SAA.Index, SAA },
            { Patriot.Index, Patriot },
            { Scorpion.Index, Scorpion },
            { XM16E1.Index, XM16E1 },
            { AK47.Index, AK47 },
            { M63.Index, M63 },
            { M37.Index, M37 },
            { SVD.Index, SVD },
            { MosinNagant.Index, MosinNagant },
            { RPG7.Index, RPG7 },
            { Torch.Index, Torch },
            { Grenade.Index, Grenade },
            { WpGrenade.Index, WpGrenade },
            { StunGrenade.Index, StunGrenade },
            { ChaffGrenade.Index, ChaffGrenade },
            { SmokeGrenade.Index, SmokeGrenade },
            { EmptyMagazine.Index, EmptyMagazine },
            { TNT.Index, TNT },
            { C3.Index, C3 },
            { Claymore.Index, Claymore },
            { Book.Index, Book },
            { Mousetrap.Index, Mousetrap },
            { DirectionalMic.Index, DirectionalMic }
        };

        public static readonly Item LifeMedicine = new Item("Life Medicine", 0);
        public static readonly Item Pentazemin = new Item("Pentazemin", 1);
        public static readonly Item FakeDeathPill = new Item("Fake Death Pill", 2);
        public static readonly Item RevivalPill = new Item("Revival Pill", 3);
        public static readonly Item Cigar = new Item("Cigar", 4);
        public static readonly Item Binoculars = new Item("Binoculars", 5);
        public static readonly Item ThermalGoggles = new Item("Thermal Goggles", 6);
        public static readonly Item NightVisionGoggles = new Item("Night Vision Goggles", 7);
        public static readonly Item Camera = new Item("Camera", 8);
        public static readonly Item MotionDetector = new Item("Motion Detector", 9);
        public static readonly Item ActiveSonar = new Item("Active Sonar", 10);
        public static readonly Item MineDetector = new Item("Mine Detector", 11);
        public static readonly Item AntiPersonnelSensor = new Item("Anti Personnel Sensor", 12);
        public static readonly Item CBoxA = new Item("CBox A", 13);
        public static readonly Item CBoxB = new Item("CBox B", 14);
        public static readonly Item CBoxC = new Item("CBox C", 15);
        public static readonly Item CBoxD = new Item("CBox D", 16);
        public static readonly Item CrocCap = new Item("Croc Cap", 17);
        public static readonly Item KeyA = new Item("Key A", 18);
        public static readonly Item KeyB = new Item("Key B", 19);
        public static readonly Item KeyC = new Item("Key C", 20);
        public static readonly Item Bandana = new Item("Bandana", 21);
        public static readonly Item StealthCamo = new Item("Stealth Camo", 22);
        public static readonly Item BugJuice = new Item("Bug Juice", 23);
        public static readonly Item MonkeyMask = new Item("Monkey Mask", 24);
        // New Delta Items
        public static readonly Item AtCamo = new Item("AT Camo", 25);
        public static readonly Item Compass = new Item("Compass", 26);
        public static readonly Item Itm2 = new Item("ITM2", 27);
        public static readonly Item GakoMask = new Item("Gako Mask", 28);
        public static readonly Item KerotanMask = new Item("Kerotan Mask", 29);
        public static readonly Item BananaCapGold = new Item("Banana Cap (Gold)", 30);
        public static readonly Item BombCapGold = new Item("Bomb Cap (Gold)", 31);
        public static readonly Item BombCap = new Item("Bomb Cap", 32);
        public static readonly Item CrocCapOneEye = new Item("Croc Cap (One Eyed)", 33);
        public static readonly Item Itm9 = new Item("ITM9", 34);
        // Medicinal Items
        public static readonly Item Serum = new Item("Serum", 35);
        public static readonly Item Antidote = new Item("Antidote", 36);
        public static readonly Item ColdMedicine = new Item("Cold Medicine", 37);
        public static readonly Item DigestiveMedicine = new Item("Digestive Medicine", 38);
        public static readonly Item Ointment = new Item("Ointment", 39);
        public static readonly Item Splint = new Item("Splint", 40);
        public static readonly Item Disinfectant = new Item("Disinfectant", 41);
        public static readonly Item Styptic = new Item("Styptic", 42);
        public static readonly Item Bandage = new Item("Bandage", 43);
        public static readonly Item SutureKit = new Item("Suture Kit", 44);
        // Items that are not technically items but are in this table
        public static readonly Item Knife = new Item("Knife", 45);
        public static readonly Item Battery = new Item("Battery", 46);
        public static readonly Item M1911A1Suppressor = new Item("M1911A1 Suppressor", 47);
        public static readonly Item MK22Suppressor = new Item("MK22 Suppressor", 48);
        public static readonly Item XM16E1Suppressor = new Item("XM16E1 Suppressor", 49);


        public static readonly Dictionary<int, Item> AllItems = new Dictionary<int, Item>
        {
            { LifeMedicine.Index, LifeMedicine },
            { Pentazemin.Index, Pentazemin },
            { FakeDeathPill.Index, FakeDeathPill },
            { RevivalPill.Index, RevivalPill },
            { Cigar.Index, Cigar },
            { Binoculars.Index, Binoculars },
            { ThermalGoggles.Index, ThermalGoggles },
            { NightVisionGoggles.Index, NightVisionGoggles },
            { Camera.Index, Camera },
            { MotionDetector.Index, MotionDetector },
            { ActiveSonar.Index, ActiveSonar },
            { MineDetector.Index, MineDetector },
            { AntiPersonnelSensor.Index, AntiPersonnelSensor },
            { CBoxA.Index, CBoxA },
            { CBoxB.Index, CBoxB },
            { CBoxC.Index, CBoxC },
            { CBoxD.Index, CBoxD },
            { CrocCap.Index, CrocCap },
            { KeyA.Index, KeyA },
            { KeyB.Index, KeyB },
            { KeyC.Index, KeyC },
            { Bandana.Index, Bandana },
            { StealthCamo.Index, StealthCamo },
            { BugJuice.Index, BugJuice },
            { MonkeyMask.Index, MonkeyMask },
            // New Delta Items
            { AtCamo.Index, AtCamo },
            { Compass.Index, Compass },
            { Itm2.Index, Itm2 },
            { GakoMask.Index, GakoMask },
            { KerotanMask.Index, KerotanMask },
            { BananaCapGold.Index, BananaCapGold },
            { BombCapGold.Index, BombCapGold },
            { BombCap.Index, BombCap },
            { CrocCapOneEye.Index, CrocCapOneEye },
            { Itm9.Index, Itm9 },
            // Medicinal Items
            { Serum.Index, Serum },
            { Antidote.Index, Antidote },
            { ColdMedicine.Index, ColdMedicine },
            { DigestiveMedicine.Index, DigestiveMedicine },
            { Ointment.Index, Ointment },
            { Splint.Index, Splint },
            { Disinfectant.Index, Disinfectant },
            { Styptic.Index, Styptic },
            { Bandage.Index, Bandage },
            { SutureKit.Index, SutureKit },
            // Additional items
            { Knife.Index, Knife },
            { Battery.Index, Battery },
            { M1911A1Suppressor.Index, M1911A1Suppressor },
            { MK22Suppressor.Index, MK22Suppressor },
            { XM16E1Suppressor.Index, XM16E1Suppressor },
        };
    }

    #endregion

    #region Enums

    // Unused in Delta CC currently but if we find a way to force the survival viewer open to visually change Snake's clothes these are still relevant
    public enum SnakesUniformCamo
    {
        OliveDrab = 0,
        TigerStripe = 1,
        Leaf = 2,
        TreeBark = 3,
        ChocoChip = 4,
        Splitter = 5,
        Raindrop = 6,
        Squares = 7,
        Water = 8,
        Black = 9,
        Snow = 10,
        Naked = 11,
        SneakingSuit = 12,
        Scientist = 13,
        Officer = 14,
        Maintenance = 15,
        Tuxedo = 16,
        HornetStripe = 17,
        Spider = 18,
        Moss = 19,
        Fire = 20,
        Spirit = 21,
        ColdWar = 22,
        Snake = 23,
        GakoCamo = 24,
        DesertTiger = 25,
        DPM = 26,
        Flecktarn = 27,
        Auscam = 28,
        Animals = 29,
        Fly = 30,
        BananaCamo = 31
    }

    public enum SnakesFacePaint
    {
        NoPaint = 0,
        Woodland = 1,
        Black = 2,
        Water = 3,
        Desert = 4,
        Splitter = 5,
        Snow = 6,
        Kabuki = 7,
        Zombie = 8,
        Oyama = 9,
        Mask = 10, // Causes crashes when forced on or off will need an exception like if mask is on then don't change
        Green = 11,
        Brown = 12,
        Infinity = 13,
        SovietUnion = 14,
        UK = 15,
        France = 16,
        Germany = 17,
        Italy = 18,
        Spain = 19,
        Sweden = 20,
        Japan = 21,
        USA = 22
    }

    #endregion

    #region Memory Getters and Setters

    byte Get8(AddressChain addr)
    {
        return addr.GetByte();
    }

    void Set8(AddressChain addr, byte val)
    {
        addr.SetByte(val);
    }

    short Get16(AddressChain addr)
    {
        return BitConverter.ToInt16(addr.GetBytes(2), 0);
    }

    void Set16(AddressChain addr, short val)
    {
        addr.SetBytes(BitConverter.GetBytes(val));
    }

    int Get32(AddressChain addr)
    {
        return BitConverter.ToInt32(addr.GetBytes(4), 0);
    }

    void Set32(AddressChain addr, int val)
    {
        addr.SetBytes(BitConverter.GetBytes(val));
    }

    float GetFloat(AddressChain addr)
    {
        if (addr.TryGetBytes(4, out byte[] bytes))
        {
            return BitConverter.ToSingle(bytes, 0);
        }
        else
        {
            throw new Exception("Failed to read float value.");
        }
    }

    void SetFloat(AddressChain addr, float val)
    {
        byte[] bytes = BitConverter.GetBytes(val);
        addr.SetBytes(bytes);
    }

    T[] GetArray<T>(AddressChain addr, int count) where T : struct
    {
        int typeSize = System.Runtime.InteropServices.Marshal.SizeOf<T>();
        int totalSize = typeSize * count;
        byte[] bytes = addr.GetBytes(totalSize);

        T[] array = new T[count];
        Buffer.BlockCopy(bytes, 0, array, 0, totalSize);
        return array;
    }

    void SetArray<T>(AddressChain addr, T[] values) where T : struct
    {
        int typeSize = System.Runtime.InteropServices.Marshal.SizeOf<T>();
        int totalSize = typeSize * values.Length;
        byte[] bytes = new byte[totalSize];
        Buffer.BlockCopy(values, 0, bytes, 0, totalSize);
        addr.SetBytes(bytes);
    }

    public static short SetSpecificBits(short currentValue, int startBit, int endBit, int valueToSet)
    {
        int maskLength = endBit - startBit + 1;
        int mask = ((1 << maskLength) - 1) << startBit;
        return (short)((currentValue & ~mask) | ((valueToSet << startBit) & mask));
    }

    private string GetString(AddressChain addr, int maxLength, bool unicode = false)
    {
        if (maxLength <= 0)
            return string.Empty;

        try
        {
            int bytesToRead = unicode ? maxLength * 2 : maxLength;
            byte[] data = addr.GetBytes(bytesToRead);

            if (data == null || data.Length == 0)
                return string.Empty;

            if (unicode)
            {
                // Find UTF-16 null terminator (two zero bytes)
                for (int i = 0; i < data.Length - 1; i += 2)
                {
                    if (data[i] == 0 && data[i + 1] == 0)
                    {
                        return Encoding.Unicode.GetString(data, 0, i);
                    }
                }
                // No null terminator found - use all data (ensuring even length)
                int length = data.Length - (data.Length % 2);
                return Encoding.Unicode.GetString(data, 0, length);
            }
            else
            {
                // Find ASCII null terminator (single zero byte)
                int nullIndex = Array.IndexOf(data, (byte)0);
                if (nullIndex >= 0)
                {
                    return Encoding.ASCII.GetString(data, 0, nullIndex);
                }
                // No null terminator found - use all data
                return Encoding.ASCII.GetString(data, 0, data.Length);
            }
        }
        catch
        {
            return string.Empty;
        }
    }

    #endregion

    #region Effect Helpers

    #region Snake's Weapons

    private Weapon GetCurrentEquippedWeapon()
    {
        byte weaponId = Get8(snakeCurrentEquippedWeapon);
        if (MetalGearSolidDeltaUsableObjects.AllWeapons.TryGetValue(weaponId, out Weapon weapon))
        {
            return weapon;
        }
        else
        {
            Log.Error($"Unknown weapon ID: {weaponId}");
            return null;
        }
    }

    private void SetSnakeCurrentWeaponToNone()
    {
        try
        {
            Log.Message("Attempting to set Snake's Current Weapon to None.");
            byte originalWeapon = Get8(snakeCurrentEquippedWeapon);
            Set8(snakeCurrentEquippedWeapon, (byte)MetalGearSolidDeltaUsableObjects.NoneWeapon.Index);
            byte newWeapon = Get8(snakeCurrentEquippedWeapon);
            Log.Message($"Original Weapon was {originalWeapon}, new Weapon is {newWeapon}.");
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Snake's Current Weapon: {ex.Message}");
        }
    }

    private bool TrySubtractAmmoFromCurrentWeapon(short amount)
    {
        try
        {
            Weapon weapon = GetCurrentEquippedWeapon();
            if (weapon == null || !weapon.HasAmmo)
            {
                Log.Message($"{weapon?.Name ?? "Unknown Weapon"} does not use ammo.");
                return false;
            }

            var ammoAddress = weapon.GetPropertyAddress(baseWeaponAddress, WeaponAddresses.CurrentAmmoOffset);
            short currentAmmo = Get16(ammoAddress);

            if (currentAmmo <= 0)
            {
                Log.Message($"{weapon.Name} has no ammo to subtract.");
                return false;
            }

            short newAmmo = (short)Math.Max(currentAmmo - amount, 0);

            if (newAmmo == currentAmmo)
            {
                Log.Message($"{weapon.Name} ammo cannot be reduced further.");
                return false;
            }

            Set16(ammoAddress, newAmmo);
            Log.Message($"Subtracted {amount} ammo from {weapon.Name}. Ammo: {currentAmmo} -> {newAmmo}");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while subtracting ammo: {ex.Message}");
            return false;
        }
    }

    private bool TryAddAmmoToCurrentWeapon(short amount)
    {
        try
        {
            Weapon weapon = GetCurrentEquippedWeapon();
            if (weapon == null || !weapon.HasAmmo)
            {
                Log.Message($"{weapon?.Name ?? "Unknown Weapon"} does not use ammo.");
                return false;
            }

            var ammoAddress = weapon.GetPropertyAddress(baseWeaponAddress, WeaponAddresses.CurrentAmmoOffset);
            short currentAmmo = Get16(ammoAddress);
            short maxAmmo = Get16(weapon.GetPropertyAddress(baseWeaponAddress, WeaponAddresses.MaxAmmoOffset));

            if (currentAmmo >= maxAmmo)
            {
                Log.Message($"{weapon.Name} ammo is already full.");
                return false;
            }

            short newAmmo = (short)Math.Min(currentAmmo + amount, maxAmmo);

            if (newAmmo == currentAmmo)
            {
                Log.Message($"{weapon.Name} ammo cannot be increased further.");
                return false;
            }

            Set16(ammoAddress, newAmmo);
            Log.Message($"Added {amount} ammo to {weapon.Name}. Ammo: {currentAmmo} -> {newAmmo}");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while adding ammo: {ex.Message}");
            return false;
        }
    }

    private void EmptySnakeClip()
    {
        try
        {
            Weapon weapon = GetCurrentEquippedWeapon();
            if (weapon == null || !weapon.HasClip)
            {
                Log.Message($"{weapon?.Name ?? "Unknown Weapon"} does not have a clip.");
                return;
            }

            var clipAddress = weapon.GetPropertyAddress(baseWeaponAddress, WeaponAddresses.ClipOffset);
            Set16(clipAddress, 0);
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while emptying clip: {ex.Message}");
        }
    }

    private void SetTacticalReloadEnabled(bool enabled)
    {
        try
        {
            byte[] instruction = enabled ?
                new byte[] { 0x66, 0x89, 0x4A, 0x2C } : // Normal
                new byte[] { 0x90, 0x90, 0x90, 0x90 };  // Disabled

            SetArray(snakeTacticalReloadInstructions, instruction);
            Log.Message($"Tactical reload {(enabled ? "enabled" : "disabled")}");
        }
        catch (Exception ex)
        {
            Log.Error($"Error setting tactical reload: {ex.Message}");
        }
    }

    private void SetManualReloadEnabled(bool enabled)
    {
        try
        {
            byte[] instruction = enabled ?
                new byte[] { 0x66, 0x89, 0x48, 0x2C } : // Normal
                new byte[] { 0x90, 0x90, 0x90, 0x90 };  // Disabled

            SetArray(snakeManualReloadInstructions, instruction);
            Log.Message($"Manual reload {(enabled ? "enabled" : "disabled")}");
        }
        catch (Exception ex)
        {
            Log.Error($"Error setting manual reload: {ex.Message}");
        }
    }

    private bool IsTacticalReloadEnabled()
    {
        try
        {
            byte[] normalInstruction = new byte[] { 0x66, 0x89, 0x4A, 0x2C };
            byte[] currentInstruction = GetArray<byte>(snakeTacticalReloadInstructions, 4);
            return currentInstruction.SequenceEqual(normalInstruction);
        }
        catch (Exception ex)
        {
            Log.Error($"Error checking tactical reload: {ex.Message}");
            return false;
        }
    }

    private bool IsManualReloadEnabled()
    {
        try
        {
            byte[] normalInstruction = new byte[] { 0x66, 0x89, 0x48, 0x2C };
            byte[] currentInstruction = GetArray<byte>(snakeManualReloadInstructions, 4);
            return currentInstruction.SequenceEqual(normalInstruction);
        }
        catch (Exception ex)
        {
            Log.Error($"Error checking manual reload: {ex.Message}");
            return false;
        }
    }

    private short GetWeaponValue(Weapon weapon)
    {
        try
        {
            var weaponAddress = weapon.GetPropertyAddress(baseWeaponAddress, WeaponAddresses.CurrentAmmoOffset);
            return Get16(weaponAddress);
        }
        catch (Exception ex)
        {
            Log.Error($"Error reading {weapon.Name} value: {ex.Message}");
            return -1;
        }
    }

    private bool GiveWeapon(Weapon weapon)
    {
        try
        {
            var weaponAddress = weapon.GetPropertyAddress(baseWeaponAddress, WeaponAddresses.CurrentAmmoOffset);
            short currentValue = Get16(weaponAddress);

            if (currentValue == 1)
            {
                Log.Message($"{weapon.Name} is already in inventory.");
                return false;
            }

            Set16(weaponAddress, 1);
            Log.Message($"Added {weapon.Name} to inventory.");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"Error giving {weapon.Name}: {ex.Message}");
            return false;
        }
    }

    private bool RemoveWeapon(Weapon weapon)
    {
        try
        {
            var weaponAddress = weapon.GetPropertyAddress(baseWeaponAddress, WeaponAddresses.CurrentAmmoOffset);
            short currentValue = Get16(weaponAddress);

            if ((currentValue == 0) || (currentValue == -1))
            {
                Log.Message($"{weapon.Name} is not in inventory.");
                return false;
            }

            Set16(weaponAddress, -1);
            Log.Message($"Removed {weapon.Name} from inventory.");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"Error removing {weapon.Name}: {ex.Message}");
            return false;
        }
    }

    private bool AddAmmoToWeapon(Weapon weapon, short amount)
    {
        try
        {
            if (!weapon.HasAmmo)
            {
                Log.Message($"{weapon.Name} does not use ammo.");
                return false;
            }

            var ammoAddress = weapon.GetPropertyAddress(baseWeaponAddress, WeaponAddresses.CurrentAmmoOffset);
            short currentAmmo = Get16(ammoAddress);
            short maxAmmo = Get16(weapon.GetPropertyAddress(baseWeaponAddress, WeaponAddresses.MaxAmmoOffset));

            // Handles if a weapon is not obtained yet since the value is -1 this way the correct ammo is given
            if (currentAmmo == -1)
            {
                currentAmmo = 0;
            }

            short newAmmo = (short)Math.Min(currentAmmo + amount, maxAmmo);

            if (newAmmo == currentAmmo)
            {
                Log.Message($"{weapon.Name} ammo cannot be increased further.");
                return false;
            }

            Set16(ammoAddress, newAmmo);
            Log.Message($"Added {amount} ammo to {weapon.Name}. Ammo: {currentAmmo} -> {newAmmo}");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"Error adding ammo to {weapon.Name}: {ex.Message}");
            return false;
        }
    }

    private bool SubtractAmmoFromWeapon(Weapon weapon, short amount)
    {
        try
        {
            if (!weapon.HasAmmo)
            {
                Log.Message($"{weapon.Name} does not use ammo.");
                return false;
            }

            var ammoAddress = weapon.GetPropertyAddress(baseWeaponAddress, WeaponAddresses.CurrentAmmoOffset);
            short currentAmmo = Get16(ammoAddress);

            if (currentAmmo <= 0)
            {
                Log.Message($"{weapon.Name} has no ammo to subtract.");
                return false;
            }

            short newAmmo = (short)Math.Max(currentAmmo - amount, 0);

            if (newAmmo == currentAmmo)
            {
                Log.Message($"{weapon.Name} ammo cannot be reduced further.");
                return false;
            }

            Set16(ammoAddress, newAmmo);
            Log.Message($"Subtracted {amount} ammo from {weapon.Name}. Ammo: {currentAmmo} -> {newAmmo}");

            // If ammo reaches 0 and weapon has a clip, empty the clip too
            if (newAmmo == 0 && weapon.HasClip)
            {
                var clipAddress = weapon.GetPropertyAddress(baseWeaponAddress, WeaponAddresses.ClipOffset);
                Set16(clipAddress, 0);
                Log.Message($"Also emptied {weapon.Name} clip since ammo reached 0.");
            }

            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"Error subtracting ammo from {weapon.Name}: {ex.Message}");
            return false;
        }
    }

    private bool AddMaxAmmoToWeapon(Weapon weapon, short amount, short minValue)
    {
        try
        {
            if (!weapon.HasAmmo)
            {
                Log.Message($"{weapon.Name} does not use ammo.");
                return false;
            }

            var maxAmmoAddress = weapon.GetPropertyAddress(baseWeaponAddress, WeaponAddresses.MaxAmmoOffset);
            short currentMaxAmmo = Get16(maxAmmoAddress);
            short newMaxAmmo = (short)Math.Min(currentMaxAmmo + amount, 999);

            Set16(maxAmmoAddress, newMaxAmmo);
            Log.Message($"Increased {weapon.Name} max ammo by {amount}. Max Ammo: {currentMaxAmmo} -> {newMaxAmmo}");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"Error increasing max ammo for {weapon.Name}: {ex.Message}");
            return false;
        }
    }

    private bool RemoveMaxAmmoFromWeapon(Weapon weapon, short amount, short minValue)
    {
        try
        {
            if (!weapon.HasAmmo)
            {
                Log.Message($"{weapon.Name} does not use ammo.");
                return false;
            }

            var maxAmmoAddress = weapon.GetPropertyAddress(baseWeaponAddress, WeaponAddresses.MaxAmmoOffset);
            var currentAmmoAddress = weapon.GetPropertyAddress(baseWeaponAddress, WeaponAddresses.CurrentAmmoOffset);

            short currentMaxAmmo = Get16(maxAmmoAddress);
            short currentAmmo = Get16(currentAmmoAddress);
            short newMaxAmmo = (short)Math.Max(currentMaxAmmo - amount, minValue);

            // If current ammo exceeds new max just reduce the current ammo to match new max
            if (currentAmmo > newMaxAmmo)
            {
                Set16(currentAmmoAddress, newMaxAmmo);
                Log.Message($"Adjusted current ammo to match new max: {currentAmmo} -> {newMaxAmmo}");
            }

            Set16(maxAmmoAddress, newMaxAmmo);
            Log.Message($"Decreased {weapon.Name} max ammo by {amount}. Max Ammo: {currentMaxAmmo} -> {newMaxAmmo}");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"Error decreasing max ammo for {weapon.Name}: {ex.Message}");
            return false;
        }
    }

    #endregion

    #region Snake's Items

    private short GetItemValue(Item item)
    {
        try
        {
            AddressChain currentAddress = item.GetPropertyAddress(baseItemAddress, ItemAddresses.CurrentCapacityOffset);
            return Get16(currentAddress);
        }
        catch (Exception ex)
        {
            Log.Error($"Error reading current value for {item.Name}: {ex.Message}");
            return -1;
        }
    }

    private short GetItemMaxCapacity(Item item)
    {
        try
        {
            AddressChain maxAddress = item.GetPropertyAddress(baseItemAddress, ItemAddresses.MaxCapacityOffset);
            return Get16(maxAddress);
        }
        catch (Exception ex)
        {
            Log.Error($"Error reading max capacity for {item.Name}: {ex.Message}");
            return -1;
        }
    }

    private void SetItemValue(Item item, short newValue)
    {
        AddressChain currentAddress = item.GetPropertyAddress(baseItemAddress, ItemAddresses.CurrentCapacityOffset);
        Set16(currentAddress, newValue);
        Log.Message($"{item.Name} value set to {newValue}");
    }

    #endregion

    #region Snake's Stats

    private void SetSnakeStamina()
    {
        try
        {
            Log.Message("Attempting to set Snake's Stamina to 0.");

            short originalStamina = Get16(snakeStamina);
            Set16(snakeStamina, 0);
            short newStamina = Get16(snakeStamina);

            Log.Message($"Original Stamina was {originalStamina}, new Stamina is {newStamina}.");
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Snake's Stamina: {ex.Message}");
        }
    }

    private void SetSnakeMaxStamina()
    {
        try
        {
            Log.Message($"Attempting to set Snake's Stamina to 30000.");
            short originalStamina = Get16(snakeStamina);
            Set16(snakeStamina, 30000);
            short newStamina = Get16(snakeStamina);
            Log.Message($"Original Stamina was {originalStamina}, new Stamina is {newStamina}.");
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Snake's Stamina: {ex.Message}");
        }
    }

    private void SetSnakeHealthToZero()
    {
        try
        {
            Set16(snakeHealth, 0);
            Log.Message("Set Snake's health to 0.");
        }
        catch (Exception ex)
        {
            Log.Error($"Error setting Snake's health to 0: {ex.Message}");
        }
    }

    private void FillSnakeHealth()
    {
        try
        {
            short maxHealth = Get16(snakeMaxHealth);
            Set16(snakeHealth, maxHealth);
            Log.Message($"Filled Snake's health to max: {maxHealth}");
        }
        catch (Exception ex)
        {
            Log.Error($"Error filling Snake's health: {ex.Message}");
        }
    }

    private bool IncreaseSnakeMaxHealth(short multiplier)
    {
        try
        {
            short currentMaxHealth = Get16(snakeMaxHealth);
            short amountToAdd = (short)(5 * multiplier);
            short newMaxHealth = (short)Math.Min(currentMaxHealth + amountToAdd, 400);

            Set16(snakeMaxHealth, newMaxHealth);
            Log.Message($"Increased Snake's max health by {amountToAdd}. Max Health: {currentMaxHealth} -> {newMaxHealth}");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"Error increasing Snake's max health: {ex.Message}");
            return false;
        }
    }

    private bool DecreaseSnakeMaxHealth(short multiplier)
    {
        try
        {
            short currentMaxHealth = Get16(snakeMaxHealth);
            short amountToSubtract = (short)(5 * multiplier);
            short newMaxHealth = (short)Math.Max(currentMaxHealth - amountToSubtract, 90);

            Set16(snakeMaxHealth, newMaxHealth);
            Log.Message($"Decreased Snake's max health by {amountToSubtract}. Max Health: {currentMaxHealth} -> {newMaxHealth}");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"Error decreasing Snake's max health: {ex.Message}");
            return false;
        }
    }

    private void SetDrunkModeEnabled(bool enabled)
    {
        try
        {
            byte[] instruction = enabled ?
                new byte[] { 0x90, 0x90, 0x90, 0x90 } : // Drunk
                new byte[] { 0x0F, 0x29, 0x67, 0x10 };  // Normal

            SetArray(everyoneIsDrunk, instruction);
        }
        catch (Exception ex)
        {
            Log.Error($"Error setting drunk mode: {ex.Message}");
        }
    }

    private void SnakeHasTheCommonCold()
    {
        try
        {
            Log.Message("Attempting to give Snake the common cold.");
            byte[] coldArray = new byte[] { 0, 0, 100, 0, 0, 0, 0, 0, 12, 4, 0, 0, 44, 1 };
            SetArray(snakeCommonCold, coldArray);
            Log.Message("Snake has the common cold.");
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while giving Snake the common cold: {ex.Message}");
        }
    }

    private void SnakeIsPoisoned()
    {
        try
        {
            Log.Message("Attempting to poison Snake.");
            byte[] poisonArray = new byte[] { 0, 0, 100, 0, 0, 0, 0, 0, 10, 2, 0, 0, 44, 1 };
            SetArray(snakePoison, poisonArray);
            Log.Message("Snake is poisoned.");
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while poisoning Snake: {ex.Message}");
        }
    }

    private void SnakeHasFoodPoisoning()
    {
        try
        {
            Log.Message("Attempting to give Snake food poisoning.");
            byte[] foodPoisoningArray = new byte[] { 10, 0, 100, 0, 10, 0, 0, 0, 13, 1, 0, 0, 43, 1 };
            SetArray(snakeFoodPoisoning, foodPoisoningArray);
            Log.Message("Snake has food poisoning.");
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while giving Snake food poisoning: {ex.Message}");
        }
    }

    private void SnakeHasLeeches()
    {
        try
        {
            Log.Message("Attempting to give Snake leeches.");
            byte[] leechesArray = new byte[] { 171, 255, 117, 255, 119, 0, 253, 127, 7, 0, 0, 0, 44, 1 };
            SetArray(snakeHasLeeches, leechesArray);
            Log.Message("Snake has leeches.");
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while giving Snake leeches: {ex.Message}");
        }
    }

    private void SnakeHasABulletBee()
    {
        try
        {
            Log.Message("Attempting to give Snake a Bullet Bee.");
            byte[] leechesArray = new byte[] { 123, 255, 251, 255, 20, 0, 250, 67, 6, 2, 0, 0, 1, 0 };
            SetArray(snakeHasLeeches, leechesArray);
            Log.Message("Snake has a Bullet Bee.");
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while giving Snake a bullet bee: {ex.Message}");
        }
    }

    private void SnakeHasATranqDart()
    {
        try
        {
            Log.Message("Attempting to give Snake a Tranq Dart.");
            byte[] leechesArray = new byte[] { 221, 255, 204, 0, 75, 0, 33, 122, 9, 2, 0, 0, 33, 0 };
            SetArray(snakeHasLeeches, leechesArray);
            Log.Message("Snake has a Tranq Dart.");
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while giving Snake a bullet bee: {ex.Message}");
        }
    }

    private void SetSnakeDamageMultiplierInstruction()
    {
        try
        {
            byte[] currentBytes = GetArray<byte>(snakeDamageMultiplierInstructions, 62);
            byte[] expectedBytes = new byte[] {
            0x66, 0xBD, 0x00, 0x00, 0x66, 0x0F, 0xAF, 0xC5, 0x66, 0x31, 0xED, 0xEB, 0x18, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x66, 0x89, 0x93, 0x80, 0x2F, 0x00, 0x00, 0x48, 0x8B, 0x0D, 0x34, 0x25, 0xA6, 0x04, 0x4C, 0x8D, 0x0D, 0x35, 0xD2, 0x52, 0x02, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 };

            bool needsUpdate = false;
            for (int i = 0; i < currentBytes.Length; i++)
            {
                if (i != 2 && currentBytes[i] != expectedBytes[i])
                {
                    needsUpdate = true;
                    break;
                }
            }

            if (needsUpdate)
            {
                byte currentMultiplier = currentBytes[2];

                SetArray(snakeDamageMultiplierInstructions, expectedBytes);

                Set8(snakeDamageMultiplierInstructions + 2, currentMultiplier);

            }
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while checking Snake's Damage Multiplier Instruction: {ex.Message}");
        }
    }

    private bool CanSetSnakeDamageMultiplier()
    {
        try
        {
            short currentValue = Get16(snakeDamageMultiplierValue);
            // Allow setting if current value is one of these "inactive" states
            return currentValue == 1 || currentValue == 0 || currentValue == 29760 || currentValue == 64;
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while checking Snake's Damage Multiplier: {ex.Message}");
            return false;
        }
    }

    private void SetSnakeDamageMultiplierValue(int value)
    {
        try
        {
            // Only apply protection if we're trying to set an "active" multiplier value (2, 3, 4, 5)
            // and check if we're currently in an inactive state
            if ((value == 2 || value == 3 || value == 4 || value == 5) && !CanSetSnakeDamageMultiplier())
            {
                return;
            }

            short originalValue = Get16(snakeDamageMultiplierValue);
            Set16(snakeDamageMultiplierValue, (short)value);
            short newValue = Get16(snakeDamageMultiplierValue);

        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Snake's Damage Multiplier Value: {ex.Message}");
        }
    }

    private void SetSnakeCamoIndexInstructionToNormal()
    {
        try
        {
            byte[] camoIndexInstruction = new byte[] { 0x8B, 0x05, 0x42, 0x5D, 0x8B, 0x0B };
            SetArray(camoIndexInstructions, camoIndexInstruction);
            byte[] camoIndexInstruction2 = new byte[] { 0x8B, 0x05, 0xA9, 0xDE, 0xB0, 0x0B };
            SetArray(camoIndexInstructions2, camoIndexInstruction2);
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Snake's Camo Index Instruction: {ex.Message}");
        }
    }

    private void SetSnakeCamoIndexInstructionToWritable()
    {
        try
        {
            byte[] camoIndexInstruction = new byte[] { 0x8B, 0x05, 0x36, 0xFF, 0xFF, 0xFF }; // Patched instruction 1
            SetArray(camoIndexInstructions, camoIndexInstruction);
            byte[] camoIndexInstruction2 = new byte[] { 0x8B, 0x05, 0x6D, 0x69, 0x25, 0x00 }; // Patched instruction 2
            SetArray(camoIndexInstructions2, camoIndexInstruction2);
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Snake's Camo Index Instruction: {ex.Message}");
        }
    }

    private void SetSnakeCamoIndexValue(int value)
    {
        try
        {
            int originalValue = Get32(camoIndexValue);
            Set32(camoIndexValue, value);
            int newValue = Get32(camoIndexValue);
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Snake's Camo Index Value: {ex.Message}");
        }
    }

    private bool IsCamoIndexInstructionNormal()
    {
        try
        {
            byte[] patchedInstruction1 = new byte[] { 0x8B, 0x05, 0x36, 0xFF, 0xFF, 0xFF };
            byte[] patchedInstruction2 = new byte[] { 0x8B, 0x05, 0x6D, 0x69, 0x25, 0x00 };
            byte[] currentInstruction1 = GetArray<byte>(camoIndexInstructions, 6);
            byte[] currentInstruction2 = GetArray<byte>(camoIndexInstructions2, 6);

            // Return true if NEITHER instruction matches the patched bytes
            return !currentInstruction1.SequenceEqual(patchedInstruction1) &&
                   !currentInstruction2.SequenceEqual(patchedInstruction2);
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while checking Camo Index Instruction: {ex.Message}");
            return false;
        }
    }

    #endregion

    #region Guard Stats

    #region Lethal Damage

    private void SetGuardLethalDamageInvincible()
    {
        try
        {
            SetArray(snakeWeaponDamage, new byte[] { 0x74, 0x0F, 0x0F, 0xB7, 0x42, 0x08, 0x48, 0x83, 0xC2, 0x08, 0x66, 0x85, 0xC0, 0x79, 0xED, 0x33, 0xC0, 0xF3, 0x0F, 0x59, 0xC1, 0x49, 0xBC, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x3F, 0x66, 0x4D, 0x0F, 0x6E, 0xD4, 0xF3, 0x0F, 0x5A, 0xC0, 0xF2, 0x41, 0x0F, 0x59, 0xC2, 0x90, 0x90, 0x90, 0x90, 0xF2, 0x0F, 0x2C, 0xC0, 0xC3 });
            SetArray(snakeWeaponDamageMulti, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            Set32(flameDamage, 9000);
            Set32(guardThroatSlitDamage, 9000);
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Guard Lethal Damage to Oneshot: {ex.Message}");
        }
    }

    private void SetGuardLethalDamageVeryStrong()
    {
        try
        {
            SetArray(snakeWeaponDamage, new byte[] { 0x74, 0x0F, 0x0F, 0xB7, 0x42, 0x08, 0x48, 0x83, 0xC2, 0x08, 0x66, 0x85, 0xC0, 0x79, 0xED, 0x33, 0xC0, 0xF3, 0x0F, 0x59, 0xC1, 0x49, 0xBC, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x3F, 0x66, 0x4D, 0x0F, 0x6E, 0xD4, 0xF3, 0x0F, 0x5A, 0xC0, 0xF2, 0x41, 0x0F, 0x59, 0xC2, 0x90, 0x90, 0x90, 0x90, 0xF2, 0x0F, 0x2C, 0xC0, 0xC3 });
            SetArray(snakeWeaponDamageMulti, new byte[] { 0x9A, 0x99, 0x99, 0x99, 0x99, 0x99, 0xB9, 0x3F });
            Set32(flameDamage, 0);
            Set32(guardThroatSlitDamage, 0);
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Guard Lethal Damage to Oneshot: {ex.Message}");
        }
    }

    private void SetGuardLethalDamageDefault()
    {
        try
        {
            SetArray(snakeWeaponDamage, new byte[] { 0x74, 0x0F, 0x0F, 0xB7, 0x42, 0x08, 0x48, 0x83, 0xC2, 0x08, 0x66, 0x85, 0xC0, 0x79, 0xED, 0x33, 0xC0, 0xF3, 0x0F, 0x59, 0xC1, 0x49, 0xBC, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x3F, 0x66, 0x4D, 0x0F, 0x6E, 0xD4, 0xF3, 0x0F, 0x5A, 0xC0, 0xF2, 0x41, 0x0F, 0x59, 0xC2, 0x90, 0x90, 0x90, 0x90, 0xF2, 0x0F, 0x2C, 0xC0, 0xC3 });
            SetArray(snakeWeaponDamageMulti, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x3F });
            Set32(flameDamage, 0);
            Set32(guardThroatSlitDamage, 0);
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Guard Lethal Damage to Oneshot: {ex.Message}");
        }
    }

    private void SetGuardLethalDamageVeryWeak()
    {
        try
        {
            SetArray(snakeWeaponDamage, new byte[] { 0x74, 0x0F, 0x0F, 0xB7, 0x42, 0x08, 0x48, 0x83, 0xC2, 0x08, 0x66, 0x85, 0xC0, 0x79, 0xED, 0x33, 0xC0, 0xF3, 0x0F, 0x59, 0xC1, 0x49, 0xBC, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x3F, 0x66, 0x4D, 0x0F, 0x6E, 0xD4, 0xF3, 0x0F, 0x5A, 0xC0, 0xF2, 0x41, 0x0F, 0x59, 0xC2, 0x90, 0x90, 0x90, 0x90, 0xF2, 0x0F, 0x2C, 0xC0, 0xC3 });
            SetArray(snakeWeaponDamageMulti, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40 });
            Set32(flameDamage, 0);
            Set32(guardThroatSlitDamage, 0);
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Guard Lethal Damage to Oneshot: {ex.Message}");
        }
    }

    private void SetGuardLethalDamageOneshot()
    {
        try
        {
            SetArray(snakeWeaponDamage, new byte[] { 0x74, 0x0F, 0x0F, 0xB7, 0x42, 0x08, 0x48, 0x83, 0xC2, 0x08, 0x66, 0x85, 0xC0, 0x79, 0xED, 0x33, 0xC0, 0xF3, 0x0F, 0x59, 0xC1, 0x49, 0xBC, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x3F, 0x66, 0x4D, 0x0F, 0x6E, 0xD4, 0xF3, 0x0F, 0x5A, 0xC0, 0xF2, 0x41, 0x0F, 0x59, 0xC2, 0x90, 0x90, 0x90, 0x90, 0xF2, 0x0F, 0x2C, 0xC0, 0xC3 });
            SetArray(snakeWeaponDamageMulti, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x24, 0x40 });
            Set32(flameDamage, 0);
            Set32(guardThroatSlitDamage, 0);
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Guard Lethal Damage to Oneshot: {ex.Message}");
        }
    }


    #endregion

    #region Sleep Damage

    private void SetGuardSleepDamageAlmostInvincible()
    {
        try
        {
            Set32(sleepTimer1, 90000);
            Set32(sleepTimer2, 90000);
            Set32(sleepTimer3, 90000);
            SetArray(sleepDrain, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
            SetArray(tranqHead, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
            SetArray(tranqBody, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Guard Sleep Damage to Invincible: {ex.Message}");
        }
    }

    private void SetGuardSleepDamageVeryStrong()
    {
        try
        {
            Set32(sleepTimer1, -1000);
            Set32(sleepTimer2, -1000);
            Set32(sleepTimer3, -1000);
            SetArray(sleepDrain, new byte[] { 0x89, 0x87, 0x48, 0x01, 0x00, 0x00 });
            SetArray(tranqHead, new byte[] { 0x44, 0x29, 0xB6, 0x48, 0x01, 0x00, 0x00 });
            SetArray(tranqBody, new byte[] { 0x89, 0x8B, 0x48, 0x01, 0x00, 0x00 });
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Guard Sleep Damage to Very Strong: {ex.Message}");
        }
    }

    private void SetGuardSleepDamageDefault()
    {
        try
        {
            Set32(sleepTimer1, -90000);
            Set32(sleepTimer2, -54000);
            Set32(sleepTimer3, -36000);
            SetArray(sleepDrain, new byte[] { 0x89, 0x87, 0x48, 0x01, 0x00, 0x00 });
            SetArray(tranqHead, new byte[] { 0x44, 0x29, 0xB6, 0x48, 0x01, 0x00, 0x00 });
            SetArray(tranqBody, new byte[] { 0x89, 0x8B, 0x48, 0x01, 0x00, 0x00 });
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Guard Sleep Damage to Default: {ex.Message}");
        }
    }

    private void SetGuardSleepDamageVeryWeak()
    {
        try
        {
            Set32(sleepTimer1, -40000);
            Set32(sleepTimer2, -40000);
            Set32(sleepTimer3, -40000);
            SetArray(sleepDrain, new byte[] { 0x89, 0x87, 0x48, 0x01, 0x00, 0x00 });
            SetArray(tranqHead, new byte[] { 0x44, 0x29, 0xB6, 0x48, 0x01, 0x00, 0x00 });
            SetArray(tranqBody, new byte[] { 0x89, 0x8B, 0x48, 0x01, 0x00, 0x00 });
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Guard Sleep Damage to Very Weak: {ex.Message}");
        }
    }

    private void SetGuardSleepDamageOneshot()
    {
        try
        {
            Set32(sleepTimer1, -1000000);
            Set32(sleepTimer2, -1000000);
            Set32(sleepTimer3, -1000000);
            SetArray(sleepDrain, new byte[] { 0x89, 0x87, 0x48, 0x01, 0x00, 0x00 });
            SetArray(tranqHead, new byte[] { 0x44, 0x29, 0xB6, 0x48, 0x01, 0x00, 0x00 });
            SetArray(tranqBody, new byte[] { 0x89, 0x8B, 0x48, 0x01, 0x00, 0x00 });
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Guard Sleep Damage to Oneshot: {ex.Message}");
        }
    }

    #endregion

    #region Stun Damage

    private void SetGuardStunAlmostDamageInvincible()
    {
        try
        {
            Set32(stunTimer1, 90000);
            Set32(stunTimer2, 90000);
            Set32(stunTimer3, 90000);
            SetArray(stunPunch, new byte[] { 0x29, 0x86, 0x40, 0x01, 0x00, 0x00 });
            SetArray(stunGrenade, new byte[] { 0x29, 0x86, 0x40, 0x01, 0x00, 0x00 });
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Guard Stun Damage to Almost Invincible: {ex.Message}");
        }
    }

    private void SetGuardStunDamageVeryStrong()
    {
        try
        {
            Set32(stunTimer1, -1000);
            Set32(stunTimer2, -1000);
            Set32(stunTimer3, -1000);
            SetArray(stunPunch, new byte[] { 0x29, 0x86, 0x40, 0x01, 0x00, 0x00 });
            SetArray(stunGrenade, new byte[] { 0x29, 0x86, 0x40, 0x01, 0x00, 0x00 });
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Guard Stun Damage to Very Strong: {ex.Message}");
        }
    }

    private void SetGuardStunDamageDefault()
    {
        try
        {
            Set32(stunTimer1, -90000);
            Set32(stunTimer2, -54000);
            Set32(stunTimer3, -36000);
            SetArray(stunPunch, new byte[] { 0x29, 0x86, 0x40, 0x01, 0x00, 0x00 });
            SetArray(stunGrenade, new byte[] { 0x29, 0x86, 0x40, 0x01, 0x00, 0x00 });
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Guard Stun Damage to Default: {ex.Message}");
        }
    }

    private void SetGuardStunDamageVeryWeak()
    {
        try
        {
            Set32(stunTimer1, -40000);
            Set32(stunTimer2, -40000);
            Set32(stunTimer3, -40000);
            SetArray(stunPunch, new byte[] { 0x29, 0x86, 0x40, 0x01, 0x00, 0x00 });
            SetArray(stunGrenade, new byte[] { 0x29, 0x86, 0x40, 0x01, 0x00, 0x00 });
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Guard Stun Damage to Very Weak: {ex.Message}");
        }
    }

    private void SetGuardStunDamageOneshot()
    {
        try
        {
            Set32(stunTimer1, -1000000);
            Set32(stunTimer2, -1000000);
            Set32(stunTimer3, -1000000);
            SetArray(stunPunch, new byte[] { 0x29, 0x86, 0x40, 0x01, 0x00, 0x00 });
            SetArray(stunGrenade, new byte[] { 0x29, 0x86, 0x40, 0x01, 0x00, 0x00 });
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Guard Stun Damage to One Shot: {ex.Message}");
        }
    }

    #endregion

    #endregion

    #region Alert Status


    private bool SetAlertStatus()
    {
        try
        {
            short alertTimerValue = Get16(alertTimer);
            if (alertTimerValue != 0)
            {
                Log.Message("Cannot set Alert status - alert timer is already active");
                return false;
            }

            Log.Message("Forcing Alert status...");
            Set8(alertStatus, 144);
            Log.Message("Alert status forced.");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"Error forcing Alert: {ex.Message}");
            return false;
        }
    }

    private bool SetEvasionStatus()
    {
        try
        {
            short alertTimerValue = Get16(alertTimer);
            short evasionTimerValue = Get16(evasionTimer);

            if (alertTimerValue != 0 || evasionTimerValue != 0)
            {
                Log.Message("Cannot set Evasion status - alert or evasion timer is active");
                return false;
            }

            short current = Get16(alertStatus);
            short cleared = SetSpecificBits(current, 6, 15, 400);
            Set16(alertStatus, cleared);

            short evasionValue = Get16(alertStatus);
            short newValue = SetSpecificBits(evasionValue, 5, 14, 596);
            Set16(alertStatus, newValue);

            Log.Message($"Successfully forced Evasion. Old short: {current}, new short: {newValue}.");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while forcing Evasion: {ex.Message}");
            return false;
        }
    }

    private bool SetCautionStatus()
    {
        try
        {
            short alertTimerValue = Get16(alertTimer);
            short evasionTimerValue = Get16(evasionTimer);
            short cautionTimerValue = Get16(cautionTimer);

            if (alertTimerValue != 0 || evasionTimerValue != 0 || cautionTimerValue != 0)
            {
                Log.Message("Cannot set Caution status - alert, evasion, or caution timer is active");
                return false;
            }

            Log.Message("Forcing Caution status...");
            Set8(alertStatus, 32);
            Log.Message("Caution status forced.");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"Error forcing Caution: {ex.Message}");
            return false;
        }
    }
    #endregion

    #region Filters

    private void SetFogEnabled(bool enabled)
    {
        try
        {
            byte[] instruction = enabled ?
                new byte[] { 0x83, 0x78, 0x04, 0x01 } : // Normal fog
                new byte[] { 0x90, 0x90, 0x90, 0x90 };  // Fog removed

            SetArray(fogFilter, instruction);
            Log.Message($"Fog filter {(enabled ? "enabled" : "disabled")}");
        }
        catch (Exception ex)
        {
            Log.Error($"Error setting fog filter: {ex.Message}");
        }
    }

    private bool IsFogEnabled()
    {
        try
        {
            byte[] normalInstruction = new byte[] { 0x83, 0x78, 0x04, 0x01 };
            byte[] currentInstruction = GetArray<byte>(fogFilter, 4);
            return currentInstruction.SequenceEqual(normalInstruction);
        }
        catch (Exception ex)
        {
            Log.Error($"Error checking fog filter: {ex.Message}");
            return false;
        }
    }

    private void SetFilterValues(


        byte[] filterInstructionsValue,
        float filterRValue,
        float filterGValue,
        float filterBValue,
        float filterAValue,

        byte[] lightColourInstructionsValue,
        float lightColourRValue,
        float lightColourGValue,
        float lightColourBValue,
        float lightColourAValue

        )
    {
        SetArray(filterInstructions, filterInstructionsValue);
        SetFloat(filterR, filterRValue);
        SetFloat(filterG, filterGValue);
        SetFloat(filterB, filterBValue);
        SetFloat(filterA, filterAValue);

        SetArray(lightColourInstructions, lightColourInstructionsValue);
        SetFloat(lightColourR, lightColourRValue);
        SetFloat(lightColourG, lightColourGValue);
        SetFloat(lightColourB, lightColourBValue);
        SetFloat(lightColourA, lightColourAValue);
    }

    public void SetToPissFilterMode()
    {
        byte[] filterInstructionsValue = new byte[] {
        0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,
        0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,
        0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,
        0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90
    };

        byte[] lightColourInstructionsValue = new byte[] {
        0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,
        0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,
        0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,
        0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90
    };

        SetFilterValues(
            filterInstructionsValue,
            2.0f,    // filterRValue
            2.0f,    // filterGValue  
            2.0f,    // filterBValue
            0.7f,    // filterAValue
            lightColourInstructionsValue,
            1.0f,    // lightColourRValue
            1.0f,    // lightColourGValue
            2.0f,    // lightColourBValue
            0.9f     // lightColourAValue
        );
    }

    #endregion

    #region Snake's Stats and Active Equipment

    private void MakeSnakeSleep()
    {
        try
        {
            Log.Message("Attempting to make Snake sleep.");
            Set8(snakeSleeps, 1);
            Log.Message("Snake is sleeping.");
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while making Snake sleep: {ex.Message}");
        }
    }

    private void MakeSnakeTrip()
    {
        try
        {
            Log.Message("Attempting to make Snake trip.");
            Set8(snakeTrips, 2);
            Log.Message("Snake has tripped.");
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while making Snake trip: {ex.Message}");
        }
    }

    private void MakeSnakePukeFire()
    {
        try
        {
            Log.Message("Attempting to make Snake puke while being set on fire.");
            Set8(snakePukeFire, 255);
            Log.Message("Snake is puking and on fire.");
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while making Snake puke fire: {ex.Message}");
        }
    }

    private void MakeSnakePuke()
    {
        try
        {
            Log.Message("Attempting to make Snake puke.");
            Set8(snakePukeFire, 1);
            Thread.Sleep(1500);
            Log.Message("Snake is puking.");
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while making Snake puke: {ex.Message}");
        }
    }

    private void SetSnakeOnFire()
    {
        try
        {
            Log.Message("Attempting to set Snake on fire.");
            Set8(snakePukeFire, 8);
            Log.Message("Snake is on fire.");
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Snake on fire: {ex.Message}");
        }
    }

    private bool CheckAndUnpauseGame()
    {
        var currentValue = Get32(isPausedOrMenu);

        if (currentValue == 4)
        {
            Set32(isPausedOrMenu, 0);
            return true;
        }
        return false;
    }

    #endregion

    #region Game State Tracking

    private bool IsReady(EffectRequest request, bool ignoreMenuCheck = false)
    {
        try
        {
            byte gameState = Get8(isPausedOrMenu);
            if (gameState == 1)
            {
                Log.Message("Game is paused or on the radio.");
                return false;
            }

            else if (gameState == 4 && !ignoreMenuCheck)
            {
                Log.Message("Game is in the weapon/item selection menu.");
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while checking game state: {ex.Message}");
            return false;
        }
    }

    private bool IsInCutscene()
    {
        try
        {
            string currentMap = GetString(mapStringAddress, 64).Trim().ToLowerInvariant();
            var cutsceneMaps = new List<string> { "kyle_op", "title", "theater", "ending" };

            // Cutscene have _0 or _1 at the end of the map name only hud related effects should be allowed during cutscenes
            if (cutsceneMaps.Contains(currentMap) || currentMap.EndsWith("_0") || currentMap.EndsWith("_1"))
            {
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Log.Error("Error in IsInCutscene: " + ex.Message);
            return true;
        }
    }

    private bool IsMedicalItemEffectsAllowed()
    {
        try
        {
            string currentMap = GetString(mapStringAddress, 32).Trim().ToLowerInvariant();
            var medicalItemMaps = new List<string> { "v001a", "v003a", "v004a", "v005a", "v006a", "v006b", "v007a" };

            if (medicalItemMaps.Contains(currentMap))
            {
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            Log.Error("Error in IsMedicalItemEffectsAllowed: " + ex.Message);
            return false;
        }
    }

    private bool IsSleepAllowedOnCurrentMap()
    {
        try
        {
            string currentMap = GetString(mapStringAddress, 64).Trim().ToLowerInvariant();
            var noSleepMaps = new List<string> { "v001a", "v003a", "v004a", "v005a", "v006a", "v006b", "v007a", "s012a", "s066a", "s161a", "s162a", "s163a", "s163b", "s171a", "s171b", "s181a", "s182a", "s183a" };

            if (noSleepMaps.Contains(currentMap))
            {
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            Log.Error("Error in IsSleepAllowedOnCurrentMap: " + ex.Message);
            return false;
        }
    }

    private bool IsAlertAllowedOnCurrentMap()
    {
        try
        {
            string currentMap = GetString(mapStringAddress, 64).Trim().ToLowerInvariant();

            var noAlertMaps = new List<string> { "v001a", "s002a", "v003a", "v007a", "v006b", "s003a", "s012a", "s023a", "s031a", "s032a", "s032b", "s033a", "s051a", "s051b", "s066a", "s113a", "s152a", "s161a", "s162a", "s163a", "s163b", "s171a", "s171b", "s181a", "s182a", "s183a" };

            if (noAlertMaps.Contains(currentMap))
            {
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            Log.Error("Error in IsAlertMapAllowed: " + ex.Message);
            return false;
        }
    }

    #endregion

    #endregion

    #region Crowd Control Effects - Pricing, Descriptions, Etc

    public override EffectList Effects => new List<Effect>
    {
    
    #region Snake's Active Equipment

    new ("- Decrease Active Weapon Ammo", "subtractAmmo")
        {
        Price = 2,
        Quantity = 100,
        Description = "Removes a chunk of Snake's ammunition from his equipped weapon",
        Category = "Snake's Active Equipment"
        },

    new ("+ Increase Active Weapon Ammo", "addAmmo")
        {
        Price = 2,
        Quantity = 100,
        Description = "Grants additional ammunition to the gun Snake has equipped",
        Category = "Snake's Active Equipment"
        },

    new ("Empty Snake's Weapon Clip", "emptyCurrentWeaponClip")
        {
        Price = 80,
        Duration = 8,
        Description = "Forces Snake to reload over and over for a short time",
        Category = "Snake's Active Equipment"
        },

    new ("Remove Snake's Weapon", "setSnakeCurrentWeaponToNone")
        {
        Price = 30,
        Duration = 5,
        Description = "Leaves Snake to fight with CQC only by unequipping his current weapon for a short time",
        Category = "Snake's Active Equipment"
        },

    new ("Remove Suppressors", "removeCurrentSuppressor")
        {
        Price = 30,
        Duration = 10,
        Description = "Removes the suppressor from all of Snake's guns for a short while",
        Category = "Snake's Active Equipment"
        },

    new ("Camo Index to -100%", "setSnakeCamoIndexNegative")
        {
        Price = 150,
        Duration = 45,
        Description = "Sets Snake's camo index to -100 for a limited time",
        Category = "Snake's Active Equipment"
        },

    new ("Camo Index to 100%", "setSnakeCamoIndexPositive")
        {
        Price = 150,
        Duration = 45,
        Description = "Sets Snake's camo index to 100 for a limited time",
        Category = "Snake's Active Equipment"
        },

    new ("Real Time Item Swap", "realTimeItemSwap")
        {
        Price = 50,
        Duration = 30,
        Description = "For a short while Snake can no longer stop time when switching weapons and items",
        Category = "Snake's Active Equipment"
        },

    new ("Snake and Friends are Drunk", "everyoneIsDrunk")
        {
        Price = 30,
        Duration = 30,
        Description = "For a short while Snake and everyone move around like they're drunk",
        Category = "Snake's Active Equipment"
        },
               
    #endregion

        #region Snake's Stats

        new ("- Max Health", "decreaseSnakeMaxHealth")
        {
        Price = 150,
        Quantity = 100,
        Description = "Decrease Snake's Max Health Bar",
        Category = "Snake's Stats"
        },

    new ("+ Max Health", "increaseSnakeMaxHealth")
        {
        Price = 150,
        Quantity = 100,
        Description = "Increase Snake's Max Health Bar",
        Category = "Snake's Stats"
        },

    new ("Stamina to 0", "setSnakeStamina")
        {
        Price = 150,
        Description = "Drains Snake's stamina completely",
        Category = "Snake's Stats"
        },

    new ("Stamina to Full", "setSnakeMaxStamina")
        {
        Price = 150,
        Description = "Fully restores Snake's health bar to it's current max amount",
        Category = "Snake's Stats"
        },

    new ("Health to 0", "setSnakeHealthZero")
        {
        Price = 500,
        Description = "Drains Snake's health completely only having a life med equipped can save him",
        Category = "Snake's Stats"
        },

    new ("Health to Full", "fillSnakeHealth")
        {
        Price = 500,
        Description = "Fully restores Snake's stamina bar",
        Category = "Snake's Stats"
        },

        new ("Snake Falls Over", "MakeSnakeTrip")
        {
        Price = 50,
        Description = "Makes Snake fall over for a moment",
        Category = "Snake's Stats"
        },

    new ("Snake Nap Time", "MakeSnakeSleep")
        {
        Price = 500,
        Description = "Puts Snake to sleep instantly for about 20 seconds",
        Category = "Snake's Stats"
        },

    new ("Snake Pukes and gets set on Fire", "makeSnakePukeFire")
        {
        Price = 250,
        Description = "Causes Snake to vomit explosively and catch fire",
        Category = "Snake's Stats"
        },

    new ("Snake Pukes", "makeSnakePuke")
        {
        Price = 100,
        Description = "Causes Snake to vomit once he stands still",
        Category = "Snake's Stats"
        },

    new ("Snake is on Fire", "setSnakeOnFire")
        {
        Price = 150,
        Description = "Sets Snake on fire, causing him to take damage over time",
        Category = "Snake's Stats"
        },
        #endregion
        
    #region Snake's Medical Conditions

    new ("Common Cold", "snakeHasTheCommonCold")
        {
        Price = 30,
        Description = "Inflicts Snake with a cold, causing sneezes to alert enemies and draining stamina until cured",
        Category = "Snake's Medical Conditions"
        },

    new ("Poison", "snakeIsPoisoned")
        {
        Price = 150,
        Description = "Poisons Snake, slowly draining his health",
        Category = "Snake's Medical Conditions"
        },

    new ("Food Poisoning", "snakeHasFoodPoisoning")
        {
        Price = 40,
        Description = "Gives Snake food poisoning, causing frequent nausea until cured or if he pukes",
        Category = "Snake's Medical Conditions"
        },

    new ("Leeches", "snakeHasLeeches")
        {
        Price = 40,
        Description = "Attaches leeches to Snake, draining stamina until removed",
        Category = "Snake's Medical Conditions"
        },

    new ("Bullet Bee", "snakeHasABulletBee")
        {
        Price = 70,
        Description = "Attaches leeches to Snake, draining health for a short time unless removed first",
        Category = "Snake's Medical Conditions"
        },

    new ("Tranq Dart", "snakeHasATranqDart")
        {
        Price = 70,
        Description = "Puts a tranq dart in Snake, draining stamina until removed",
        Category = "Snake's Medical Conditions"
        },

        #endregion

    #region Alert Status Effects

    new ("Alert Status", "setAlertStatus")
        {
        Price = 100,
        Description = "Triggers an alert status, sending the enemies to attack Snake",
        Category = "Alert Status"
        },

    new ("Evasion Status", "setEvasionStatus")
        {
        Price = 60,
        Description = "Puts the guards into evasion mode, where guards actively search for Snake",
        Category = "Alert Status"
        },

    new ("Caution Status", "setCautionStatus")
        {
        Price = 30,
        Description = "Puts the guards into caution mode with heightened awareness",
        Category = "Alert Status"
        },

        #endregion

    #region Filter Effects
    
    new ("Remove Fog", "removeFog")
    {
        Price = 10,
        Description = "Removes fog from the game for better visibility",
        Category = "Filters and Visuals"
    },

    new ("Restore Fog", "restoreFog")
    {
        Price = 10,
        Description = "Restores fog effects to the game",
        Category = "Filters and Visuals"
    },

    new ("ANTIBigBoss's Homebrewed Piss Filter", "setPissFilter")
        {
        Price = 10,
        Description = "Go back to 2004 with that piss stained filter every PS2 era game had.",
        Category = "Filters and Visuals"
        },

        #endregion

    #region Weapons
    
    new ("Remove Survival Knife", "removeSurvivalKnife")
        {
        Price = 30,
        Duration = 30,
        Description = "Removes Snake's knife for a short time",
        Category = "Weapons",
        Image = "remove_item"
        },

    new ("Add Fork", "giveFork")
        {
        Price = 20,
        Description = "Gives Snake a fork so he can eat without the survival viewer",
        Category = "Weapons",
        Image = "give_item"
        },

    new ("Remove Fork", "removeFork")
        {
        Price = 20,
        Description = "Removes Snake's fork, no more eating all the jungle food for Snake",
        Category = "Weapons",
        Image = "remove_item"
        },

    new ("Add Ez Gun", "giveEzGun")
        {
        Price = 100,
        Description = "Gives Snake the Ez Gun which makes the mission super easy for him",
        Category = "Weapons",
        Image = "give_item"
        },

    new ("Remove Ez Gun", "removeEzGun")
        {
        Price = 100,
        Description = "Removes Snake's Ez Gun which makes life more difficult for him",
        Category = "Weapons",
        Image = "remove_item"
        },

    new ("Add Patriot", "givePatriot")
        {
        Price = 100,
        Description = "A Patriot... Why are you giving Snake this",
        Category = "Weapons",
        Image = "give_item"
        },

    new ("Remove Patriot", "removePatriot")
        {
        Price = 100,
        Description = "Removes Snake's Patriots which was his last memento of The Boss",
        Category = "Weapons",
        Image = "remove_item"
        },

    new ("+ Increase MK22 Ammo", "addMK22Ammo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Increases ammo for Snake's MK22 tranquilizer pistol",
            Category = "Weapons",
            Image = "give_item"
        },

    new ("- Decrease MK22 Ammo", "removeMK22Ammo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Decreases ammo for Snake's MK22 tranquilizer pistol",
            Category = "Weapons",
            Image = "remove_item"
        },

    new ("+ Increase M1911A1 Ammo", "addM1911A1Ammo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Increases ammo for Snake's M1911A1 pistol",
            Category = "Weapons",
            Image = "give_item"
        },

    new ("- Decrease M1911A1 Ammo", "removeM1911A1Ammo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Decreases ammo for Snake's M1911A1 pistol",
            Category = "Weapons",
            Image = "remove_item"
        },

    new ("+ Increase SAA Ammo", "addSAAAmmo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Increases ammo for Snake's Single Action Army revolver",
            Category = "Weapons",
            Image = "give_item"
        },

    new ("- Decrease SAA Ammo", "removeSAAAmmo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Decreases ammo for Snake's Single Action Army revolver",
            Category = "Weapons",
            Image = "remove_item"
        },

    new ("+ Increase Scorpion Ammo", "addScorpionAmmo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Increases ammo for Snake's Scorpion submachine gun",
            Category = "Weapons",
            Image = "give_item"
        },

    new ("- Decrease Scorpion Ammo", "removeScorpionAmmo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Decreases ammo for Snake's Scorpion submachine gun",
            Category = "Weapons",
            Image = "remove_item"
        },

    new ("+ Increase XM16E1 Ammo", "addXM16E1Ammo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Increases ammo for Snake's XM16E1 assault rifle",
            Category = "Weapons",
            Image = "give_item"
        },

    new ("- Decrease XM16E1 Ammo", "removeXM16E1Ammo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Decreases ammo for Snake's XM16E1 assault rifle",
            Category = "Weapons",
            Image = "remove_item"
        },

    new ("+ Increase AK47 Ammo", "addAK47Ammo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Increases ammo for Snake's AK47 assault rifle",
            Category = "Weapons",
            Image = "give_item"
        },

    new ("- Decrease AK47 Ammo", "removeAK47Ammo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Decreases ammo for Snake's AK47 assault rifle",
            Category = "Weapons",
            Image = "remove_item"
        },

    new ("+ Increase M63 Ammo", "addM63Ammo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Increases ammo for Snake's M63 machine gun",
            Category = "Weapons",
            Image = "give_item"
        },

    new ("- Decrease M63 Ammo", "removeM63Ammo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Decreases ammo for Snake's M63 machine gun",
            Category = "Weapons",
            Image = "remove_item"
        },

    new ("+ Increase M37 Ammo", "addM37Ammo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Increases ammo for Snake's M37 shotgun",
            Category = "Weapons",
            Image = "give_item"
        },

    new ("- Decrease M37 Ammo", "removeM37Ammo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Decreases ammo for Snake's M37 shotgun",
            Category = "Weapons",
            Image = "remove_item"
        },

    new ("+ Increase SVD Ammo", "addSVDAmmo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Increases ammo for Snake's SVD sniper rifle",
            Category = "Weapons",
            Image = "give_item"
        },

    new ("- Decrease SVD Ammo", "removeSVDAmmo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Decreases ammo for Snake's SVD sniper rifle",
            Category = "Weapons",
            Image = "remove_item"
        },

    new ("+ Increase Mosin-Nagant Ammo", "addMosinNagantAmmo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Increases ammo for Snake's Mosin-Nagant sniper rifle",
            Category = "Weapons",
            Image = "give_item"
        },

    new ("- Decrease Mosin-Nagant Ammo", "removeMosinNagantAmmo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Decreases ammo for Snake's Mosin-Nagant sniper rifle",
            Category = "Weapons",
            Image = "remove_item"
        },

    new ("+ Increase RPG-7 Ammo", "addRPG7Ammo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Increases ammo for Snake's RPG-7 rocket launcher",
            Category = "Weapons",
            Image = "give_item"
        },

    new ("- Decrease RPG-7 Ammo", "removeRPG7Ammo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Decreases ammo for Snake's RPG-7 rocket launcher",
            Category = "Weapons",
            Image = "remove_item"
        },

    new ("+ Increase Grenades", "addGrenadeAmmo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Increases grenades in Snake's inventory",
            Category = "Weapons",
            Image = "give_item"
        },

    new ("- Decrease Grenades", "removeGrenadeAmmo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Decreases grenades in Snake's inventory",
            Category = "Weapons",
            Image = "remove_item"
        },

    new ("+ Increase WP Grenades", "addWpGrenadeAmmo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Increases white phosphorus grenades in Snake's inventory",
            Category = "Weapons",
            Image = "give_item"
        },

    new ("- Decrease WP Grenades", "removeWpGrenadeAmmo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Decreases white phosphorus grenades in Snake's inventory",
            Category = "Weapons",
            Image = "remove_item"
        },

    new ("+ Increase Stun Grenades", "addStunGrenadeAmmo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Increases stun grenades in Snake's inventory",
            Category = "Weapons",
            Image = "give_item"
        },

    new ("- Decrease Stun Grenades", "removeStunGrenadeAmmo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Decreases stun grenades in Snake's inventory",
            Category = "Weapons",
            Image = "remove_item"
        },

    new ("+ Increase Chaff Grenades", "addChaffGrenadeAmmo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Increases chaff grenades in Snake's inventory",
            Category = "Weapons",
            Image = "give_item"
        },

    new ("- Decrease Chaff Grenades", "removeChaffGrenadeAmmo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Decreases chaff grenades in Snake's inventory",
            Category = "Weapons",
            Image = "remove_item"
        },

    new ("+ Increase Smoke Grenades", "addSmokeGrenadeAmmo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Increases smoke grenades in Snake's inventory",
            Category = "Weapons",
            Image = "give_item"
        },

    new ("- Decrease Smoke Grenades", "removeSmokeGrenadeAmmo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Decreases smoke grenades in Snake's inventory",
            Category = "Weapons",
            Image = "remove_item"
        },

    new ("+ Increase Empty Magazines", "addEmptyMagazineAmmo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Increases empty magazines in Snake's inventory",
            Category = "Weapons",
            Image = "give_item"
        },

    new ("- Decrease Empty Magazines", "removeEmptyMagazineAmmo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Decreases empty magazines in Snake's inventory",
            Category = "Weapons",
            Image = "remove_item"
        },

    new ("+ Increase TNT", "addTntAmmo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Increases TNT in Snake's inventory",
            Category = "Weapons",
            Image = "give_item"
        },

    new ("- Decrease TNT", "removeTntAmmo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Decreases TNT in Snake's inventory",
            Category = "Weapons",
            Image = "remove_item"
        },

    new ("+ Increase Claymores", "addClaymoreAmmo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Increases claymore mines in Snake's inventory",
            Category = "Weapons",
            Image = "give_item"
        },

    new ("- Decrease Claymores", "removeClaymoreAmmo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Decreases claymore mines in Snake's inventory",
            Category = "Weapons",
            Image = "remove_item"
        },

    new ("+ Increase Books", "addBookAmmo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Increases books in Snake's inventory",
            Category = "Weapons",
            Image = "give_item"
        },

    new ("- Decrease Books", "removeBookAmmo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Decreases books in Snake's inventory",
            Category = "Weapons",
            Image = "remove_item"
        },

    new ("+ Increase Mousetraps", "addMousetrapAmmo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Increases mousetraps in Snake's inventory",
            Category = "Weapons",
            Image = "give_item"
        },

    new ("- Decrease Mousetraps", "removeMousetrapAmmo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Decreases mousetraps in Snake's inventory",
            Category = "Weapons",
            Image = "remove_item"
        },

    new ("+ Increase Cig Sprays", "addCigSprayAmmo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Increases cigarette sprays in Snake's inventory",
            Category = "Weapons",
            Image = "give_item"
        },

    new ("- Decrease Cig Sprays", "removeCigSprayAmmo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Decreases cigarette sprays in Snake's inventory",
            Category = "Weapons",
            Image = "remove_item"
        },

    new ("+ Increase Handkerchiefs", "addHandkerchiefAmmo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Increases handkerchiefs in Snake's inventory",
            Category = "Weapons",
            Image = "give_item"
        },

    new ("- Decrease Handkerchiefs", "removeHandkerchiefAmmo")
        {
            Price = 2,
            Quantity = 100,
            Description = "Decreases handkerchiefs in Snake's inventory",
            Category = "Weapons",
            Image = "remove_item"
        },

        #endregion

    #region Weapons Max Ammo
    
    new ("+ Increase MK22 Max Ammo", "addMK22MaxAmmo")
        {
            Price = 25,
            Quantity = 40,
            Description = "Increases maximum ammo capacity for Snake's MK22 tranquilizer pistol",
            Category = "Weapons - Max Ammo",
            Image = "give_item"
        },

    new ("- Decrease MK22 Max Ammo", "removeMK22MaxAmmo")
        {
            Price = 25,
            Quantity = 40,
            Description = "Decreases maximum ammo capacity for Snake's MK22 tranquilizer pistol",
            Category = "Weapons - Max Ammo",
            Image = "remove_item"
        },

    new ("+ Increase M1911A1 Max Ammo", "addM1911A1MaxAmmo")
        {
            Price = 30,
            Quantity = 40,
            Description = "Increases maximum ammo capacity for Snake's M1911A1 pistol",
            Category = "Weapons - Max Ammo",
            Image = "give_item"
        },

    new ("- Decrease M1911A1 Max Ammo", "removeM1911A1MaxAmmo")
        {
            Price = 30,
            Quantity = 40,
            Description = "Decreases maximum ammo capacity for Snake's M1911A1 pistol",
            Category = "Weapons - Max Ammo",
            Image = "remove_item"
        },

    new ("+ Increase SAA Max Ammo", "addSAAAMaxAmmo")
        {
            Price = 25,
            Quantity = 40,
            Description = "Increases maximum ammo capacity for Snake's Single Action Army revolver",
            Category = "Weapons - Max Ammo",
            Image = "give_item"
        },

    new ("- Decrease SAA Max Ammo", "removeSAAAMaxAmmo")
        {
            Price = 25,
            Quantity = 40,
            Description = "Decreases maximum ammo capacity for Snake's Single Action Army revolver",
            Category = "Weapons - Max Ammo",
            Image = "remove_item"
        },

    new ("+ Increase M37 Max Ammo", "addM37MaxAmmo")
        {
            Price = 45,
            Quantity = 30,
            Description = "Increases maximum ammo capacity for Snake's M37 shotgun",
            Category = "Weapons - Max Ammo",
            Image = "give_item"
        },

    new ("- Decrease M37 Max Ammo", "removeM37MaxAmmo")
        {
            Price = 45,
            Quantity = 30,
            Description = "Decreases maximum ammo capacity for Snake's M37 shotgun",
            Category = "Weapons - Max Ammo",
            Image = "remove_item"
        },

    new ("+ Increase SVD Max Ammo", "addSVDMaxAmmo")
        {
            Price = 25,
            Quantity = 40,
            Description = "Increases maximum ammo capacity for Snake's SVD sniper rifle",
            Category = "Weapons - Max Ammo",
            Image = "give_item"
        },

    new ("- Decrease SVD Max Ammo", "removeSVDMaxAmmo")
        {
            Price = 25,
            Quantity = 40,
            Description = "Decreases maximum ammo capacity for Snake's SVD sniper rifle",
            Category = "Weapons - Max Ammo",
            Image = "remove_item"
        },

    new ("+ Increase Mosin-Nagant Max Ammo", "addMosinNagantMaxAmmo")
        {
            Price = 30,
            Quantity = 40,
            Description = "Increases maximum ammo capacity for Snake's Mosin-Nagant sniper rifle",
            Category = "Weapons - Max Ammo",
            Image = "give_item"
        },

    new ("- Decrease Mosin-Nagant Max Ammo", "removeMosinNagantMaxAmmo")
        {
            Price = 30,
            Quantity = 40,
            Description = "Decreases maximum ammo capacity for Snake's Mosin-Nagant sniper rifle",
            Category = "Weapons - Max Ammo",
            Image = "remove_item"
        },

    new ("+ Increase RPG-7 Max Ammo", "addRPG7MaxAmmo")
        {
            Price = 70,
            Quantity = 25,
            Description = "Increases maximum ammo capacity for Snake's RPG-7 rocket launcher",
            Category = "Weapons - Max Ammo",
            Image = "give_item"
        },

    new ("- Decrease RPG-7 Max Ammo", "removeRPG7MaxAmmo")
        {
            Price = 70,
            Quantity = 25,
            Description = "Decreases maximum ammo capacity for Snake's RPG-7 rocket launcher",
            Category = "Weapons - Max Ammo",
            Image = "remove_item"
        },

    new ("+ Increase XM16E1 Max Ammo", "addXM16E1MaxAmmo")
        {
            Price = 10,
            Quantity = 100,
            Description = "Increases maximum ammo capacity for Snake's XM16E1 assault rifle",
            Category = "Weapons - Max Ammo",
            Image = "give_item"
        },

    new ("- Decrease XM16E1 Max Ammo", "removeXM16E1MaxAmmo")
        {
            Price = 10,
            Quantity = 100,
            Description = "Decreases maximum ammo capacity for Snake's XM16E1 assault rifle",
            Category = "Weapons - Max Ammo",
            Image = "remove_item"
        },

    new ("+ Increase AK47 Max Ammo", "addAK47MaxAmmo")
        {
            Price = 8,
            Quantity = 100,
            Description = "Increases maximum ammo capacity for Snake's AK47 assault rifle",
            Category = "Weapons - Max Ammo",
            Image = "give_item"
        },

    new ("- Decrease AK47 Max Ammo", "removeAK47MaxAmmo")
        {
            Price = 8,
            Quantity = 100,
            Description = "Decreases maximum ammo capacity for Snake's AK47 assault rifle",
            Category = "Weapons - Max Ammo",
            Image = "remove_item"
        },

    new ("+ Increase Scorpion Max Ammo", "addScorpionMaxAmmo")
        {
            Price = 5,
            Quantity = 100,
            Description = "Increases maximum ammo capacity for Snake's Scorpion submachine gun",
            Category = "Weapons - Max Ammo",
            Image = "give_item"
        },

    new ("- Decrease Scorpion Max Ammo", "removeScorpionMaxAmmo")
        {
            Price = 5,
            Quantity = 100,
            Description = "Decreases maximum ammo capacity for Snake's Scorpion submachine gun",
            Category = "Weapons - Max Ammo",
            Image = "remove_item"
        },

    new ("+ Increase M63 Max Ammo", "addM63MaxAmmo")
        {
            Price = 3,
            Quantity = 300,
            Description = "Increases maximum ammo capacity for Snake's M63 machine gun",
            Category = "Weapons - Max Ammo",
            Image = "give_item"
        },

    new ("- Decrease M63 Max Ammo", "removeM63MaxAmmo")
        {
            Price = 3,
            Quantity = 300,
            Description = "Decreases maximum ammo capacity for Snake's M63 machine gun",
            Category = "Weapons - Max Ammo",
            Image = "remove_item"
        },

    new ("+ Increase Grenade Max Ammo", "addGrenadeMaxAmmo")
        {
            Price = 40,
            Quantity = 20,
            Description = "Increases maximum grenade capacity in Snake's inventory",
            Category = "Weapons - Max Ammo",
            Image = "give_item"
        },

    new ("- Decrease Grenade Max Ammo", "removeGrenadeMaxAmmo")
        {
            Price = 40,
            Quantity = 20,
            Description = "Decreases maximum grenade capacity in Snake's inventory",
            Category = "Weapons - Max Ammo",
            Image = "remove_item"
        },

    new ("+ Increase WP Grenade Max Ammo", "addWpGrenadeMaxAmmo")
        {
            Price = 40,
            Quantity = 20,
            Description = "Increases maximum WP grenade capacity in Snake's inventory",
            Category = "Weapons - Max Ammo",
            Image = "give_item"
        },

    new ("- Decrease WP Grenade Max Ammo", "removeWpGrenadeMaxAmmo")
        {
            Price = 40,
            Quantity = 20,
            Description = "Decreases maximum WP grenade capacity in Snake's inventory",
            Category = "Weapons - Max Ammo",
            Image = "remove_item"
        },

    new ("+ Increase Stun Grenade Max Ammo", "addStunGrenadeMaxAmmo")
        {
            Price = 40,
            Quantity = 20,
            Description = "Increases maximum stun grenade capacity in Snake's inventory",
            Category = "Weapons - Max Ammo",
            Image = "give_item"
        },

    new ("- Decrease Stun Grenade Max Ammo", "removeStunGrenadeMaxAmmo")
        {
            Price = 40,
            Quantity = 20,
            Description = "Decreases maximum stun grenade capacity in Snake's inventory",
            Category = "Weapons - Max Ammo",
            Image = "remove_item"
        },

    new ("+ Increase Chaff Grenade Max Ammo", "addChaffGrenadeMaxAmmo")
        {
            Price = 40,
            Quantity = 20,
            Description = "Increases maximum chaff grenade capacity in Snake's inventory",
            Category = "Weapons - Max Ammo",
            Image = "give_item"
        },

    new ("- Decrease Chaff Grenade Max Ammo", "removeChaffGrenadeMaxAmmo")
        {
            Price = 40,
            Quantity = 20,
            Description = "Decreases maximum chaff grenade capacity in Snake's inventory",
            Category = "Weapons - Max Ammo",
            Image = "remove_item"
        },

    new ("+ Increase Smoke Grenade Max Ammo", "addSmokeGrenadeMaxAmmo")
        {
            Price = 40,
            Quantity = 20,
            Description = "Increases maximum smoke grenade capacity in Snake's inventory",
            Category = "Weapons - Max Ammo",
            Image = "give_item"
        },

    new ("- Decrease Smoke Grenade Max Ammo", "removeSmokeGrenadeMaxAmmo")
        {
            Price = 40,
            Quantity = 20,
            Description = "Decreases maximum smoke grenade capacity in Snake's inventory",
            Category = "Weapons - Max Ammo",
            Image = "remove_item"
        },

    new ("+ Increase Empty Magazine Max Ammo", "addEmptyMagazineMaxAmmo")
        {
            Price = 3,
            Quantity = 100,
            Description = "Increases maximum empty magazine capacity in Snake's inventory",
            Category = "Weapons - Max Ammo",
            Image = "give_item"
        },

    new ("- Decrease Empty Magazine Max Ammo", "removeEmptyMagazineMaxAmmo")
        {
            Price = 3,
            Quantity = 100,
            Description = "Decreases maximum empty magazine capacity in Snake's inventory",
            Category = "Weapons - Max Ammo",
            Image = "remove_item"
        },

    new ("+ Increase TNT Max Ammo", "addTntMaxAmmo")
        {
            Price = 25,
            Quantity = 30,
            Description = "Increases maximum TNT capacity in Snake's inventory",
            Category = "Weapons - Max Ammo",
            Image = "give_item"
        },

    new ("- Decrease TNT Max Ammo", "removeTntMaxAmmo")
        {
            Price = 25,
            Quantity = 30,
            Description = "Decreases maximum TNT capacity in Snake's inventory",
            Category = "Weapons - Max Ammo",
            Image = "remove_item"
        },

    new ("+ Increase Claymore Max Ammo", "addClaymoreMaxAmmo")
        {
            Price = 25,
            Quantity = 30,
            Description = "Increases maximum claymore capacity in Snake's inventory",
            Category = "Weapons - Max Ammo",
            Image = "give_item"
        },

    new ("- Decrease Claymore Max Ammo", "removeClaymoreMaxAmmo")
        {
            Price = 25,
            Quantity = 30,
            Description = "Decreases maximum claymore capacity in Snake's inventory",
            Category = "Weapons - Max Ammo",
            Image = "remove_item"
        },

    new ("+ Increase Book Max Ammo", "addBookMaxAmmo")
        {
            Price = 5,
            Quantity = 20,
            Description = "Increases maximum book capacity in Snake's inventory",
            Category = "Weapons - Max Ammo",
            Image = "give_item"
        },

    new ("- Decrease Book Max Ammo", "removeBookMaxAmmo")
        {
            Price = 5,
            Quantity = 20,
            Description = "Decreases maximum book capacity in Snake's inventory",
            Category = "Weapons - Max Ammo",
            Image = "remove_item"
        },

    new ("+ Increase Mousetrap Max Ammo", "addMousetrapMaxAmmo")
        {
            Price = 5,
            Quantity = 20,
            Description = "Increases maximum mousetrap capacity in Snake's inventory",
            Category = "Weapons - Max Ammo",
            Image = "give_item"
        },

    new ("- Decrease Mousetrap Max Ammo", "removeMousetrapMaxAmmo")
        {
            Price = 5,
            Quantity = 20,
            Description = "Decreases maximum mousetrap capacity in Snake's inventory",
            Category = "Weapons - Max Ammo",
            Image = "remove_item"
        },

    new ("+ Increase Cig Spray Max Ammo", "addCigSprayMaxAmmo")
        {
            Price = 35,
            Quantity = 30,
            Description = "Increases maximum cigarette spray capacity in Snake's inventory",
            Category = "Weapons - Max Ammo",
            Image = "give_item"
        },

    new ("- Decrease Cig Spray Max Ammo", "removeCigSprayMaxAmmo")
        {
            Price = 35,
            Quantity = 30,
            Description = "Decreases maximum cigarette spray capacity in Snake's inventory",
            Category = "Weapons - Max Ammo",
            Image = "remove_item"
        },

    new ("+ Increase Handkerchief Max Ammo", "addHandkerchiefMaxAmmo")
        {
            Price = 35,
            Quantity = 30,
            Description = "Increases maximum handkerchief capacity in Snake's inventory",
            Category = "Weapons - Max Ammo",
            Image = "give_item"
        },

    new ("- Decrease Handkerchief Max Ammo", "removeHandkerchiefMaxAmmo")
        {
            Price = 35,
            Quantity = 30,
            Description = "Decreases maximum handkerchief capacity in Snake's inventory",
            Category = "Weapons - Max Ammo",
            Image = "remove_item"
        },
    
    #endregion

    #region Items

        new ("Give Life Med", "giveLifeMedicine")
        {
        Price = 150,
        Quantity = 3,
        Description = "Gives Snake a Life Med to restore health",
        Category = "Items",
        Image = "give_item"
        },

    new ("Remove Life Med", "removeLifeMedicine")
        {
        Price = 150,
        Quantity = 3,
        Description = "Removes a Life Medicine from Snake's inventory",
        Category = "Items",
        Image = "remove_item"
        },

    new ("Give Scope", "giveScope")
        {
        Price = 20,
        Description = "Gives Snake a binoculars to scout the area",
        Category = "Items",
        Image = "give_item"
        },

    new ("Remove Scope", "removeScope")
        {
        Price = 20,
        Description = "No more long range scouting for Snake",
        Category = "Items",
        Image = "remove_item"
        },

    new ("Give Thermal Goggles", "giveThermalGoggles")
        {
        Price = 60,
        Description = "Gives Snake thermal goggles to see in the dark",
        Category = "Items",
        Image = "give_item"
        },

    new ("Remove Thermal Goggles", "removeThermalGoggles")
        {
        Price = 60,
        Description = "Take away Snake's thermal goggles which will stop him from tracking heat signatures",
        Category = "Items",
        Image = "remove_item"
        },

    new ("Give Night Vision Goggles", "giveNightVisionGoggles")
        {
        Price = 60,
        Description = "Gives Snake NVGs to see in the dark",
        Category = "Items",
        Image = "give_item"
        },

    new ("Remove Night Vision Goggles", "removeNightVisionGoggles")
        {
        Price = 60,
        Description = "Take away Snake's NVGs which will stop him from seeing in the dark. Pairs well with the effect to make it night time.",
        Category = "Items",
        Image = "remove_item"
        },

    new ("Give Motion Detector", "giveMotionDetector")
        {
        Price = 30,
        Description = "Gives Snake a motion detector to track enemy and animal movement",
        Category = "Items",
        Image = "give_item"
        },

    new ("Remove Motion Detector", "removeMotionDetector")
        {
        Price = 30,
        Description = "Take away Snake's motion detector which will stop him from tracking enemy and animal movement",
        Category = "Items",
        Image = "remove_item"
        },

    new ("Give Sonar", "giveSonar")
        {
        Price = 30,
        Description = "Gives Snake a sonar to detect enemy and animal positions",
        Category = "Items",
        Image = "give_item"
        },

    new ("Remove Sonar", "removeSonar")
        {
        Price = 30,
        Description = "Take away Snake's sonar which will stop him from detecting enemy and animal positions",
        Category = "Items",
        Image = "remove_item"
        },

    new ("Give Anti-Personnel Sensor", "giveAntiPersonnelSensor")
        {
        Price = 30,
        Description = "Gives Snake an anti-personnel sensor to detect enemy movement",
        Category = "Items",
        Image = "give_item"
        },

    new ("Remove Anti-Personnel Sensor", "removeAntiPersonnelSensor")
        {
        Price = 30,
        Description = "Take away Snake's anti-personnel sensor which will stop him from detecting enemy movement",
        Category = "Items",
        Image = "remove_item"
        },

    new ("Give Antidote", "giveAntidote")
        {
        Price = 30,
        Quantity = 30,
        Description = "Gives Snake an antidote to cure certain poisons",
        Category = "Medical Items",
        Image = "give_item_medical"
        },

    new ("Remove Antidote", "removeAntidote")
        {
        Price = 30,
        Quantity = 30,
        Description = "Removes an antidote from Snake's inventory",
        Category = "Medical Items",
        Image = "remove_item_medical"
        },

    new ("Give C Med", "giveCMed")
        {
        Price = 30,
        Quantity = 30,
        Description = "Gives Snake a C Med to cure colds",
        Category = "Medical Items",
        Image = "give_item_medical"
        },

    new ("Remove C Med", "removeCMed")
        {
        Price = 30,
        Quantity = 30,
        Description = "Removes a C Med from Snake's inventory, the common cold is a mystery hope he doesn't catch it.",
        Category = "Medical Items",
        Image = "remove_item_medical"
        },

    new ("Give D Med", "giveDMed")
        {
        Price = 30,
        Quantity = 30,
        Description = "Gives Snake a D Med to cure Snake's stomach issues",
        Category = "Medical Items",
        Image = "give_item_medical"
        },

    new ("Remove D Med", "removeDMed")
        {
        Price = 30,
        Quantity = 30,
        Description = "Removes a D Med from Snake's inventory, hope his stomach doesn't get upset somehow.",
        Category = "Medical Items",
        Image = "remove_item_medical"
        },

    new ("Give Serum", "giveSerum")
        {
        Price = 50,
        Quantity = 30,
        Description = "Gives Snake a serum to cure poison",
        Category = "Medical Items",
        Image = "give_item_medical"
        },

    new ("Remove Serum", "removeSerum")
        {
        Price = 50,
        Quantity = 30,
        Description = "Removes a serum from Snake's inventory, sure would suck if he got poisoned.",
        Category = "Medical Items",
        Image = "remove_item_medical"
        },

    new ("Give Bandage", "giveBandage")
        {
        Price = 60,
        Quantity = 30,
        Description = "Gives Snake a bandage to stop bleeding",
        Category = "Medical Items",
        Image = "give_item_medical"
        },

    new ("Remove Bandage", "removeBandage")
        {
        Price = 60,
        Quantity = 30,
        Description = "Removes a bandage from Snake's inventory, hope he doesn't get hurt.",
        Category = "Medical Items",
        Image = "remove_item_medical"
        },

    new ("Give Disinfectant", "giveDisinfectant")
        {
        Price = 30,
        Quantity = 30,
        Description = "Gives Snake a disinfectant to clean wounds",
        Category = "Medical Items",
        Image = "give_item_medical"
        },

    new ("Remove Disinfectant", "removeDisinfectant")
        {
        Price = 30,
        Quantity = 30,
        Description = "Removes a disinfectant from Snake's inventory, hope he doesn't have to worry about an infection.",
        Category = "Medical Items",
        Image = "remove_item_medical"
        },

    new ("Give Ointment", "giveOintment")
        {
        Price = 30,
        Quantity = 30,
        Description = "Gives Snake an ointment to heal burns",
        Category = "Medical Items",
        Image = "give_item_medical"
        },

    new ("Remove Ointment", "removeOintment")
        {
        Price = 30,
        Quantity = 30,
        Description = "Removes an ointment from Snake's inventory, getting burnt would not be ideal for Snake.",
        Category = "Medical Items",
        Image = "remove_item_medical"
        },

    new ("Give Splint", "giveSplint")
        {
        Price = 30,
        Quantity = 30,
        Description = "Gives Snake a splint to fix broken bones",
        Category = "Medical Items",
        Image = "give_item_medical"
        },

    new ("Remove Splint", "removeSplint")
        {
        Price = 30,
        Quantity = 30,
        Description = "Removes a splint from Snake's inventory, what are the odds he gets thrown off a bridge again breaking all his bones?",
        Category = "Medical Items",
        Image = "remove_item_medical"
        },

    new ("Give Styptic", "giveStyptic")
        {
        Price = 30,
        Quantity = 30,
        Description = "Gives Snake a styptic to stop bleeding",
        Category = "Medical Items",
        Image = "give_item_medical"
        },

    new ("Remove Styptic", "removeStyptic")
        {
        Price = 30,
        Quantity = 30,
        Description = "Removes a styptic from Snake's inventory, he probably doesn't need those.",
        Category = "Medical Items",
        Image = "remove_item_medical"
        },

    new ("Give Suture Kit", "giveSutureKit")
        {
        Price = 30,
        Quantity = 30,
        Description = "Gives Snake a suture kit to stitch up his cuts",
        Category = "Medical Items",
        Image = "give_item_medical"
        },

    new ("Remove Suture Kit", "removeSutureKit")
        {
        Price = 30,
        Quantity = 30,
        Description = "Removes a suture kit from Snake's inventory, he's a CQC expert he probably won't get stabbed.",
        Category = "Medical Items",
        Image = "remove_item_medical"
        },

    #endregion

    #region Damage to Snake Multipliers

    new ("Snake takes x2 Damage", "setSnakeDamageX2")
        {
        Price = 80,
        Duration = 30,
        Description = "Doubles the damage Snake takes for a limited time",
        Category = "Damage to Snake Multipliers"
        },

    new ("Snake takes x3 Damage", "setSnakeDamageX3")
        {
        Price = 150,
        Duration = 30,
        Description = "Triples the damage Snake takes for a limited time",
        Category = "Damage to Snake Multipliers"
        },

    new ("Snake takes x4 Damage", "setSnakeDamageX4")
        {
        Price = 250,
        Duration = 30,
        Description = "Quadruples the damage Snake takes for a limited time",
        Category = "Damage to Snake Multipliers"
        },

    new ("Snake takes x5 Damage", "setSnakeDamageX5")
        {
        Price = 350,
        Duration = 30,
        Description = "Quintuples the damage Snake takes for a limited time",
        Category = "Damage to Snake Multipliers"
        },

    #endregion

    #region Guard Stats

    new ("Guards are Almost Invincible", "setGuardStatsAlmostInvincible")
        {
        Price = 250,
        Duration = 30,
        Description = "Guards become almost invincible to lethal, sleep, and stun damage",
        Category = "Guard Stats"
        },

    new ("Guards become Very Strong", "setGuardStatsVeryStrong")
        {
        Price = 150,
        Duration = 30,
        Description = "Guards become very strong against lethal, sleep, and stun damage",
        Category = "Guard Stats"
        },

    new ("Guards become Very Weak", "setGuardStatsVeryWeak")
        {
        Price = 150,
        Duration = 30,
        Description = "Guards become very weak against lethal, sleep, and stun damage",
        Category = "Guard Stats"
        },

    new ("Guards can be One Shot", "setGuardStatsOneShot")
        {
        Price = 250,
        Duration = 30,
        Description = "Guards become one shot by lethal, sleep, and stun damage",
        Category = "Guard Stats"
        },

        #endregion
    
    };

    protected override GameState GetGameState()
    {
        try
        {
            if (!isPausedOrMenu.TryGetInt(out int v)) return GameState.WrongMode;
            // Checks if game is paused or on a radio call so we can delay effects
            if (v == 1) return GameState.WrongMode;
            return GameState.Ready;
        }
        catch { return GameState.Unknown; }
    }

    protected override void StartEffect(EffectRequest request)
    {
        var codeParams = FinalCode(request).Split('_');
        switch (codeParams[0])
        {

            #region Weapons

            case "removeSurvivalKnife":
                if (IsInCutscene() || (GetWeaponValue(MetalGearSolidDeltaUsableObjects.SurvivalKnife) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }

                // Survival Knife removal is timed only to prevent softlocks
                var knifeRemoveDuration = request.Duration;
                var knifeRemoveAct = RepeatAction(request,
                    () => true,
                    () => Connector.SendMessage($"{request.DisplayViewer} removed Snake's survival knife for {knifeRemoveDuration.TotalSeconds} seconds."),
                    TimeSpan.Zero,
                    () => IsReady(request),
                    TimeSpan.FromMilliseconds(500),
                    () => RemoveWeapon(MetalGearSolidDeltaUsableObjects.SurvivalKnife),
                    TimeSpan.FromMilliseconds(500),
                    false);

                knifeRemoveAct.WhenCompleted.Then
                (_ =>
                {
                    GiveWeapon(MetalGearSolidDeltaUsableObjects.SurvivalKnife);
                    Connector.SendMessage("Snake's survival knife has been restored.");
                });
                break;

            case "giveFork":
                if (IsInCutscene() || (GetWeaponValue(MetalGearSolidDeltaUsableObjects.Fork) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () => GiveWeapon(MetalGearSolidDeltaUsableObjects.Fork),
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake a fork."));
                break;

            case "removeFork":
                if (IsInCutscene() || (GetWeaponValue(MetalGearSolidDeltaUsableObjects.Fork) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () => RemoveWeapon(MetalGearSolidDeltaUsableObjects.Fork),
                    () => Connector.SendMessage($"{request.DisplayViewer} removed Snake's fork."));
                break;

            case "giveEzGun":
                if (IsInCutscene() || (GetWeaponValue(MetalGearSolidDeltaUsableObjects.EzGun) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () => GiveWeapon(MetalGearSolidDeltaUsableObjects.EzGun),
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake the EZ Gun."));
                break;

            case "removeEzGun":
                if (IsInCutscene() || (GetWeaponValue(MetalGearSolidDeltaUsableObjects.EzGun) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () => RemoveWeapon(MetalGearSolidDeltaUsableObjects.EzGun),
                    () => Connector.SendMessage($"{request.DisplayViewer} removed Snake's EZ Gun."));
                break;

            case "givePatriot":
                if (GetWeaponValue(MetalGearSolidDeltaUsableObjects.Patriot) == 1)
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () => GiveWeapon(MetalGearSolidDeltaUsableObjects.Patriot),
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake the EZ Gun."));
                break;

            case "removePatriot":
                if (GetWeaponValue(MetalGearSolidDeltaUsableObjects.Patriot) == 0)
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () => RemoveWeapon(MetalGearSolidDeltaUsableObjects.Patriot),
                    () => Connector.SendMessage($"{request.DisplayViewer} removed Snake's EZ Gun."));
                break;

            case "addMK22Ammo":
                TryEffect(request,
                    () => true,
                    () => AddAmmoToWeapon(MetalGearSolidDeltaUsableObjects.MK22, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} added {request.Quantity} ammo to MK22."));
                break;

            case "removeMK22Ammo":
                TryEffect(request,
                    () => true,
                    () => SubtractAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.MK22, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} removed {request.Quantity} ammo from MK22."));
                break;

            case "addM1911A1Ammo":
                TryEffect(request,
                    () => true,
                    () => AddAmmoToWeapon(MetalGearSolidDeltaUsableObjects.M1911A1, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} added {request.Quantity} ammo to M1911A1."));
                break;

            case "removeM1911A1Ammo":
                TryEffect(request,
                    () => true,
                    () => SubtractAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.M1911A1, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} removed {request.Quantity} ammo from M1911A1."));
                break;

            case "addSAAAmmo":
                TryEffect(request,
                    () => true,
                    () => AddAmmoToWeapon(MetalGearSolidDeltaUsableObjects.SAA, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} added {request.Quantity} ammo to SAA."));
                break;

            case "removeSAAAmmo":
                TryEffect(request,
                    () => true,
                    () => SubtractAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.SAA, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} removed {request.Quantity} ammo from SAA."));
                break;

            case "addScorpionAmmo":
                TryEffect(request,
                    () => true,
                    () => AddAmmoToWeapon(MetalGearSolidDeltaUsableObjects.Scorpion, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} added {request.Quantity} ammo to Scorpion."));
                break;

            case "removeScorpionAmmo":
                TryEffect(request,
                    () => true,
                    () => SubtractAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.Scorpion, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} removed {request.Quantity} ammo from Scorpion."));
                break;

            case "addXM16E1Ammo":
                TryEffect(request,
                    () => true,
                    () => AddAmmoToWeapon(MetalGearSolidDeltaUsableObjects.XM16E1, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} added {request.Quantity} ammo to XM16E1."));
                break;

            case "removeXM16E1Ammo":
                TryEffect(request,
                    () => true,
                    () => SubtractAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.XM16E1, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} removed {request.Quantity} ammo from XM16E1."));
                break;

            case "addAK47Ammo":
                TryEffect(request,
                    () => true,
                    () => AddAmmoToWeapon(MetalGearSolidDeltaUsableObjects.AK47, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} added {request.Quantity} ammo to AK47."));
                break;

            case "removeAK47Ammo":
                TryEffect(request,
                    () => true,
                    () => SubtractAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.AK47, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} removed {request.Quantity} ammo from AK47."));
                break;

            case "addM63Ammo":
                TryEffect(request,
                    () => true,
                    () => AddAmmoToWeapon(MetalGearSolidDeltaUsableObjects.M63, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} added {request.Quantity} ammo to M63."));
                break;

            case "removeM63Ammo":
                TryEffect(request,
                    () => true,
                    () => SubtractAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.M63, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} removed {request.Quantity} ammo from M63."));
                break;

            case "addM37Ammo":
                TryEffect(request,
                    () => true,
                    () => AddAmmoToWeapon(MetalGearSolidDeltaUsableObjects.M37, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} added {request.Quantity} ammo to M37."));
                break;

            case "removeM37Ammo":
                TryEffect(request,
                    () => true,
                    () => SubtractAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.M37, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} removed {request.Quantity} ammo from M37."));
                break;

            case "addSVDAmmo":
                TryEffect(request,
                    () => true,
                    () => AddAmmoToWeapon(MetalGearSolidDeltaUsableObjects.SVD, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} added {request.Quantity} ammo to SVD."));
                break;

            case "removeSVDAmmo":
                TryEffect(request,
                    () => true,
                    () => SubtractAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.SVD, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} removed {request.Quantity} ammo from SVD."));
                break;

            case "addMosinNagantAmmo":
                TryEffect(request,
                    () => true,
                    () => AddAmmoToWeapon(MetalGearSolidDeltaUsableObjects.MosinNagant, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} added {request.Quantity} ammo to Mosin-Nagant."));
                break;

            case "removeMosinNagantAmmo":
                TryEffect(request,
                    () => true,
                    () => SubtractAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.MosinNagant, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} removed {request.Quantity} ammo from Mosin-Nagant."));
                break;

            case "addRPG7Ammo":
                TryEffect(request,
                    () => true,
                    () => AddAmmoToWeapon(MetalGearSolidDeltaUsableObjects.RPG7, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} added {request.Quantity} ammo to RPG-7."));
                break;

            case "removeRPG7Ammo":
                TryEffect(request,
                    () => true,
                    () => SubtractAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.RPG7, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} removed {request.Quantity} ammo from RPG-7."));
                break;

            case "addGrenadeAmmo":
                TryEffect(request,
                    () => true,
                    () => AddAmmoToWeapon(MetalGearSolidDeltaUsableObjects.Grenade, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} added {request.Quantity} grenade(s)."));
                break;

            case "removeGrenadeAmmo":
                TryEffect(request,
                    () => true,
                    () => SubtractAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.Grenade, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} removed {request.Quantity} grenade(s)."));
                break;

            case "addWpGrenadeAmmo":
                TryEffect(request,
                    () => true,
                    () => AddAmmoToWeapon(MetalGearSolidDeltaUsableObjects.WpGrenade, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} added {request.Quantity} WP grenade(s)."));
                break;

            case "removeWpGrenadeAmmo":
                TryEffect(request,
                    () => true,
                    () => SubtractAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.WpGrenade, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} removed {request.Quantity} WP grenade(s)."));
                break;

            case "addStunGrenadeAmmo":
                TryEffect(request,
                    () => true,
                    () => AddAmmoToWeapon(MetalGearSolidDeltaUsableObjects.StunGrenade, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} added {request.Quantity} stun grenade(s)."));
                break;

            case "removeStunGrenadeAmmo":
                TryEffect(request,
                    () => true,
                    () => SubtractAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.StunGrenade, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} removed {request.Quantity} stun grenade(s)."));
                break;

            case "addChaffGrenadeAmmo":
                TryEffect(request,
                    () => true,
                    () => AddAmmoToWeapon(MetalGearSolidDeltaUsableObjects.ChaffGrenade, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} added {request.Quantity} chaff grenade(s)."));
                break;

            case "removeChaffGrenadeAmmo":
                TryEffect(request,
                    () => true,
                    () => SubtractAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.ChaffGrenade, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} removed {request.Quantity} chaff grenade(s)."));
                break;

            case "addSmokeGrenadeAmmo":
                TryEffect(request,
                    () => true,
                    () => AddAmmoToWeapon(MetalGearSolidDeltaUsableObjects.SmokeGrenade, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} added {request.Quantity} smoke grenade(s)."));
                break;

            case "removeSmokeGrenadeAmmo":
                TryEffect(request,
                    () => true,
                    () => SubtractAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.SmokeGrenade, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} removed {request.Quantity} smoke grenade(s)."));
                break;

            case "addEmptyMagazineAmmo":
                TryEffect(request,
                    () => true,
                    () => AddAmmoToWeapon(MetalGearSolidDeltaUsableObjects.EmptyMagazine, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} added {request.Quantity} empty magazine(s)."));
                break;

            case "removeEmptyMagazineAmmo":
                TryEffect(request,
                    () => true,
                    () => SubtractAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.EmptyMagazine, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} removed {request.Quantity} empty magazine(s)."));
                break;

            case "addTntAmmo":
                TryEffect(request,
                    () => true,
                    () => AddAmmoToWeapon(MetalGearSolidDeltaUsableObjects.TNT, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} added {request.Quantity} TNT."));
                break;

            case "removeTntAmmo":
                TryEffect(request,
                    () => true,
                    () => SubtractAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.TNT, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} removed {request.Quantity} TNT."));
                break;

            case "addClaymoreAmmo":
                TryEffect(request,
                    () => true,
                    () => AddAmmoToWeapon(MetalGearSolidDeltaUsableObjects.Claymore, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} added {request.Quantity} claymore(s)."));
                break;

            case "removeClaymoreAmmo":
                TryEffect(request,
                    () => true,
                    () => SubtractAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.Claymore, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} removed {request.Quantity} claymore(s)."));
                break;

            case "addBookAmmo":
                TryEffect(request,
                    () => true,
                    () => AddAmmoToWeapon(MetalGearSolidDeltaUsableObjects.Book, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} added {request.Quantity} book(s)."));
                break;

            case "removeBookAmmo":
                TryEffect(request,
                    () => true,
                    () => SubtractAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.Book, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} removed {request.Quantity} book(s)."));
                break;

            case "addMousetrapAmmo":
                TryEffect(request,
                    () => true,
                    () => AddAmmoToWeapon(MetalGearSolidDeltaUsableObjects.Mousetrap, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} added {request.Quantity} mousetrap(s)."));
                break;

            case "removeMousetrapAmmo":
                TryEffect(request,
                    () => true,
                    () => SubtractAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.Mousetrap, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} removed {request.Quantity} mousetrap(s)."));
                break;

            case "addCigSprayAmmo":
                TryEffect(request,
                    () => true,
                    () => AddAmmoToWeapon(MetalGearSolidDeltaUsableObjects.CigSpray, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} added {request.Quantity} cig spray(s)."));
                break;

            case "removeCigSprayAmmo":
                TryEffect(request,
                    () => true,
                    () => SubtractAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.CigSpray, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} removed {request.Quantity} cig spray(s)."));
                break;

            case "addHandkerchiefAmmo":
                TryEffect(request,
                    () => true,
                    () => AddAmmoToWeapon(MetalGearSolidDeltaUsableObjects.Handkerchief, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} added {request.Quantity} handkerchief(s)."));
                break;

            case "removeHandkerchiefAmmo":
                TryEffect(request,
                    () => true,
                    () => SubtractAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.Handkerchief, (short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} removed {request.Quantity} handkerchief(s)."));
                break;
            #endregion

            #region Weapons - Max Ammo

            case "addMK22MaxAmmo":
                TryEffect(request,
                    () => true,
                    () => AddMaxAmmoToWeapon(MetalGearSolidDeltaUsableObjects.MK22, (short)request.Quantity, 10),
                    () => Connector.SendMessage($"{request.DisplayViewer} increased MK22 max ammo by {request.Quantity}."));
                break;

            case "removeMK22MaxAmmo":
                TryEffect(request,
                    () => true,
                    () => RemoveMaxAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.MK22, (short)request.Quantity, 10),
                    () => Connector.SendMessage($"{request.DisplayViewer} decreased MK22 max ammo by {request.Quantity}."));
                break;

            case "addM1911A1MaxAmmo":
                TryEffect(request,
                    () => true,
                    () => AddMaxAmmoToWeapon(MetalGearSolidDeltaUsableObjects.M1911A1, (short)request.Quantity, 10),
                    () => Connector.SendMessage($"{request.DisplayViewer} increased M1911A1 max ammo by {request.Quantity}."));
                break;

            case "removeM1911A1MaxAmmo":
                TryEffect(request,
                    () => true,
                    () => RemoveMaxAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.M1911A1, (short)request.Quantity, 10),
                    () => Connector.SendMessage($"{request.DisplayViewer} decreased M1911A1 max ammo by {request.Quantity}."));
                break;

            case "addSAAAMaxAmmo":
                TryEffect(request,
                    () => true,
                    () => AddMaxAmmoToWeapon(MetalGearSolidDeltaUsableObjects.SAA, (short)request.Quantity, 6),
                    () => Connector.SendMessage($"{request.DisplayViewer} increased SAA max ammo by {request.Quantity}."));
                break;

            case "removeSAAAMaxAmmo":
                TryEffect(request,
                    () => true,
                    () => RemoveMaxAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.SAA, (short)request.Quantity, 6),
                    () => Connector.SendMessage($"{request.DisplayViewer} decreased SAA max ammo by {request.Quantity}."));
                break;

            case "addM37MaxAmmo":
                TryEffect(request,
                    () => true,
                    () => AddMaxAmmoToWeapon(MetalGearSolidDeltaUsableObjects.M37, (short)request.Quantity, 4),
                    () => Connector.SendMessage($"{request.DisplayViewer} increased M37 max ammo by {request.Quantity}."));
                break;

            case "removeM37MaxAmmo":
                TryEffect(request,
                    () => true,
                    () => RemoveMaxAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.M37, (short)request.Quantity, 4),
                    () => Connector.SendMessage($"{request.DisplayViewer} decreased M37 max ammo by {request.Quantity}."));
                break;

            case "addSVDMaxAmmo":
                TryEffect(request,
                    () => true,
                    () => AddMaxAmmoToWeapon(MetalGearSolidDeltaUsableObjects.SVD, (short)request.Quantity, 10),
                    () => Connector.SendMessage($"{request.DisplayViewer} increased SVD max ammo by {request.Quantity}."));
                break;

            case "removeSVDMaxAmmo":
                TryEffect(request,
                    () => true,
                    () => RemoveMaxAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.SVD, (short)request.Quantity, 10),
                    () => Connector.SendMessage($"{request.DisplayViewer} decreased SVD max ammo by {request.Quantity}."));
                break;

            case "addMosinNagantMaxAmmo":
                TryEffect(request,
                    () => true,
                    () => AddMaxAmmoToWeapon(MetalGearSolidDeltaUsableObjects.MosinNagant, (short)request.Quantity, 5),
                    () => Connector.SendMessage($"{request.DisplayViewer} increased Mosin-Nagant max ammo by {request.Quantity}."));
                break;

            case "removeMosinNagantMaxAmmo":
                TryEffect(request,
                    () => true,
                    () => RemoveMaxAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.MosinNagant, (short)request.Quantity, 5),
                    () => Connector.SendMessage($"{request.DisplayViewer} decreased Mosin-Nagant max ammo by {request.Quantity}."));
                break;

            case "addRPG7MaxAmmo":
                TryEffect(request,
                    () => true,
                    () => AddMaxAmmoToWeapon(MetalGearSolidDeltaUsableObjects.RPG7, (short)request.Quantity, 2),
                    () => Connector.SendMessage($"{request.DisplayViewer} increased RPG-7 max ammo by {request.Quantity}."));
                break;

            case "removeRPG7MaxAmmo":
                TryEffect(request,
                    () => true,
                    () => RemoveMaxAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.RPG7, (short)request.Quantity, 2),
                    () => Connector.SendMessage($"{request.DisplayViewer} decreased RPG-7 max ammo by {request.Quantity}."));
                break;

            case "addXM16E1MaxAmmo":
                TryEffect(request,
                    () => true,
                    () => AddMaxAmmoToWeapon(MetalGearSolidDeltaUsableObjects.XM16E1, (short)request.Quantity, 21),
                    () => Connector.SendMessage($"{request.DisplayViewer} increased XM16E1 max ammo by {request.Quantity}."));
                break;

            case "removeXM16E1MaxAmmo":
                TryEffect(request,
                    () => true,
                    () => RemoveMaxAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.XM16E1, (short)request.Quantity, 21),
                    () => Connector.SendMessage($"{request.DisplayViewer} decreased XM16E1 max ammo by {request.Quantity}."));
                break;

            case "addAK47MaxAmmo":
                TryEffect(request,
                    () => true,
                    () => AddMaxAmmoToWeapon(MetalGearSolidDeltaUsableObjects.AK47, (short)request.Quantity, 31),
                    () => Connector.SendMessage($"{request.DisplayViewer} increased AK47 max ammo by {request.Quantity}."));
                break;

            case "removeAK47MaxAmmo":
                TryEffect(request,
                    () => true,
                    () => RemoveMaxAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.AK47, (short)request.Quantity, 31),
                    () => Connector.SendMessage($"{request.DisplayViewer} decreased AK47 max ammo by {request.Quantity}."));
                break;

            case "addScorpionMaxAmmo":
                TryEffect(request,
                    () => true,
                    () => AddMaxAmmoToWeapon(MetalGearSolidDeltaUsableObjects.Scorpion, (short)request.Quantity, 31),
                    () => Connector.SendMessage($"{request.DisplayViewer} increased Scorpion max ammo by {request.Quantity}."));
                break;

            case "removeScorpionMaxAmmo":
                TryEffect(request,
                    () => true,
                    () => RemoveMaxAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.Scorpion, (short)request.Quantity, 31),
                    () => Connector.SendMessage($"{request.DisplayViewer} decreased Scorpion max ammo by {request.Quantity}."));
                break;

            case "addM63MaxAmmo":
                TryEffect(request,
                    () => true,
                    () => AddMaxAmmoToWeapon(MetalGearSolidDeltaUsableObjects.M63, (short)request.Quantity, 50),
                    () => Connector.SendMessage($"{request.DisplayViewer} increased M63 max ammo by {request.Quantity}."));
                break;

            case "removeM63MaxAmmo":
                TryEffect(request,
                    () => true,
                    () => RemoveMaxAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.M63, (short)request.Quantity, 50),
                    () => Connector.SendMessage($"{request.DisplayViewer} decreased M63 max ammo by {request.Quantity}."));
                break;

            case "addGrenadeMaxAmmo":
                TryEffect(request,
                    () => true,
                    () => AddMaxAmmoToWeapon(MetalGearSolidDeltaUsableObjects.Grenade, (short)request.Quantity, 1),
                    () => Connector.SendMessage($"{request.DisplayViewer} increased grenade max ammo by {request.Quantity}."));
                break;

            case "removeGrenadeMaxAmmo":
                TryEffect(request,
                    () => true,
                    () => RemoveMaxAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.Grenade, (short)request.Quantity, 1),
                    () => Connector.SendMessage($"{request.DisplayViewer} decreased grenade max ammo by {request.Quantity}."));
                break;

            case "addWpGrenadeMaxAmmo":
                TryEffect(request,
                    () => true,
                    () => AddMaxAmmoToWeapon(MetalGearSolidDeltaUsableObjects.WpGrenade, (short)request.Quantity, 1),
                    () => Connector.SendMessage($"{request.DisplayViewer} increased WP grenade max ammo by {request.Quantity}."));
                break;

            case "removeWpGrenadeMaxAmmo":
                TryEffect(request,
                    () => true,
                    () => RemoveMaxAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.WpGrenade, (short)request.Quantity, 1),
                    () => Connector.SendMessage($"{request.DisplayViewer} decreased WP grenade max ammo by {request.Quantity}."));
                break;

            case "addStunGrenadeMaxAmmo":
                TryEffect(request,
                    () => true,
                    () => AddMaxAmmoToWeapon(MetalGearSolidDeltaUsableObjects.StunGrenade, (short)request.Quantity, 1),
                    () => Connector.SendMessage($"{request.DisplayViewer} increased stun grenade max ammo by {request.Quantity}."));
                break;

            case "removeStunGrenadeMaxAmmo":
                TryEffect(request,
                    () => true,
                    () => RemoveMaxAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.StunGrenade, (short)request.Quantity, 1),
                    () => Connector.SendMessage($"{request.DisplayViewer} decreased stun grenade max ammo by {request.Quantity}."));
                break;

            case "addChaffGrenadeMaxAmmo":
                TryEffect(request,
                    () => true,
                    () => AddMaxAmmoToWeapon(MetalGearSolidDeltaUsableObjects.ChaffGrenade, (short)request.Quantity, 1),
                    () => Connector.SendMessage($"{request.DisplayViewer} increased chaff grenade max ammo by {request.Quantity}."));
                break;

            case "removeChaffGrenadeMaxAmmo":
                TryEffect(request,
                    () => true,
                    () => RemoveMaxAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.ChaffGrenade, (short)request.Quantity, 1),
                    () => Connector.SendMessage($"{request.DisplayViewer} decreased chaff grenade max ammo by {request.Quantity}."));
                break;

            case "addSmokeGrenadeMaxAmmo":
                TryEffect(request,
                    () => true,
                    () => AddMaxAmmoToWeapon(MetalGearSolidDeltaUsableObjects.SmokeGrenade, (short)request.Quantity, 1),
                    () => Connector.SendMessage($"{request.DisplayViewer} increased smoke grenade max ammo by {request.Quantity}."));
                break;

            case "removeSmokeGrenadeMaxAmmo":
                TryEffect(request,
                    () => true,
                    () => RemoveMaxAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.SmokeGrenade, (short)request.Quantity, 1),
                    () => Connector.SendMessage($"{request.DisplayViewer} decreased smoke grenade max ammo by {request.Quantity}."));
                break;

            case "addEmptyMagazineMaxAmmo":
                TryEffect(request,
                    () => true,
                    () => AddMaxAmmoToWeapon(MetalGearSolidDeltaUsableObjects.EmptyMagazine, (short)request.Quantity, 1),
                    () => Connector.SendMessage($"{request.DisplayViewer} increased empty magazine max ammo by {request.Quantity}."));
                break;

            case "removeEmptyMagazineMaxAmmo":
                TryEffect(request,
                    () => true,
                    () => RemoveMaxAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.EmptyMagazine, (short)request.Quantity, 1),
                    () => Connector.SendMessage($"{request.DisplayViewer} decreased empty magazine max ammo by {request.Quantity}."));
                break;

            case "addTntMaxAmmo":
                TryEffect(request,
                    () => true,
                    () => AddMaxAmmoToWeapon(MetalGearSolidDeltaUsableObjects.TNT, (short)request.Quantity, 1),
                    () => Connector.SendMessage($"{request.DisplayViewer} increased TNT max ammo by {request.Quantity}."));
                break;

            case "removeTntMaxAmmo":
                TryEffect(request,
                    () => true,
                    () => RemoveMaxAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.TNT, (short)request.Quantity, 1),
                    () => Connector.SendMessage($"{request.DisplayViewer} decreased TNT max ammo by {request.Quantity}."));
                break;

            case "addClaymoreMaxAmmo":
                TryEffect(request,
                    () => true,
                    () => AddMaxAmmoToWeapon(MetalGearSolidDeltaUsableObjects.Claymore, (short)request.Quantity, 1),
                    () => Connector.SendMessage($"{request.DisplayViewer} increased claymore max ammo by {request.Quantity}."));
                break;

            case "removeClaymoreMaxAmmo":
                TryEffect(request,
                    () => true,
                    () => RemoveMaxAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.Claymore, (short)request.Quantity, 1),
                    () => Connector.SendMessage($"{request.DisplayViewer} decreased claymore max ammo by {request.Quantity}."));
                break;

            case "addBookMaxAmmo":
                TryEffect(request,
                    () => true,
                    () => AddMaxAmmoToWeapon(MetalGearSolidDeltaUsableObjects.Book, (short)request.Quantity, 1),
                    () => Connector.SendMessage($"{request.DisplayViewer} increased book max ammo by {request.Quantity}."));
                break;

            case "removeBookMaxAmmo":
                TryEffect(request,
                    () => true,
                    () => RemoveMaxAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.Book, (short)request.Quantity, 1),
                    () => Connector.SendMessage($"{request.DisplayViewer} decreased book max ammo by {request.Quantity}."));
                break;

            case "addMousetrapMaxAmmo":
                TryEffect(request,
                    () => true,
                    () => AddMaxAmmoToWeapon(MetalGearSolidDeltaUsableObjects.Mousetrap, (short)request.Quantity, 1),
                    () => Connector.SendMessage($"{request.DisplayViewer} increased mousetrap max ammo by {request.Quantity}."));
                break;

            case "removeMousetrapMaxAmmo":
                TryEffect(request,
                    () => true,
                    () => RemoveMaxAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.Mousetrap, (short)request.Quantity, 1),
                    () => Connector.SendMessage($"{request.DisplayViewer} decreased mousetrap max ammo by {request.Quantity}."));
                break;

            case "addCigSprayMaxAmmo":
                TryEffect(request,
                    () => true,
                    () => AddMaxAmmoToWeapon(MetalGearSolidDeltaUsableObjects.CigSpray, (short)request.Quantity, 1),
                    () => Connector.SendMessage($"{request.DisplayViewer} increased cig spray max ammo by {request.Quantity}."));
                break;

            case "removeCigSprayMaxAmmo":
                TryEffect(request,
                    () => true,
                    () => RemoveMaxAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.CigSpray, (short)request.Quantity, 1),
                    () => Connector.SendMessage($"{request.DisplayViewer} decreased cig spray max ammo by {request.Quantity}."));
                break;

            case "addHandkerchiefMaxAmmo":
                TryEffect(request,
                    () => true,
                    () => AddMaxAmmoToWeapon(MetalGearSolidDeltaUsableObjects.Handkerchief, (short)request.Quantity, 1),
                    () => Connector.SendMessage($"{request.DisplayViewer} increased handkerchief max ammo by {request.Quantity}."));
                break;

            case "removeHandkerchiefMaxAmmo":
                TryEffect(request,
                    () => true,
                    () => RemoveMaxAmmoFromWeapon(MetalGearSolidDeltaUsableObjects.Handkerchief, (short)request.Quantity, 1),
                    () => Connector.SendMessage($"{request.DisplayViewer} decreased handkerchief max ammo by {request.Quantity}."));
                break;

            #endregion

            #region Snake's Active Equipment

            case "subtractAmmo":
                {
                    if (!int.TryParse(codeParams[1], out int quantity))
                    {
                        Respond(request, EffectStatus.FailTemporary, StandardErrors.CannotParseNumber, codeParams[1]);
                        break;
                    }

                    if (IsInCutscene())
                    {
                        DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                        break;
                    }

                    TryEffect(request,
                        () => true,
                        () => TrySubtractAmmoFromCurrentWeapon((short)quantity),
                        () => Connector.SendMessage($"{request.DisplayViewer} subtracted {quantity} ammo from {GetCurrentEquippedWeapon()?.Name ?? "Unknown Weapon"}."),
                        retryOnFail: false);
                    break;
                }

            case "addAmmo":
                {
                    if (!int.TryParse(codeParams[1], out int quantity) || IsInCutscene())
                    {
                        Respond(request, EffectStatus.FailTemporary, StandardErrors.CannotParseNumber, codeParams[1]);
                        break;
                    }

                    TryEffect(request,
                        () => true,
                        () => TryAddAmmoToCurrentWeapon((short)quantity),
                        () => Connector.SendMessage($"{request.DisplayViewer} added {quantity} ammo to {GetCurrentEquippedWeapon()?.Name ?? "Unknown Weapon"}."),
                        retryOnFail: false);
                    break;
                }

            case "emptyCurrentWeaponClip":
                {
                    var emptyClipDuration = request.Duration;
                    bool reloadsWereDisabled = false;

                    var emptyClipAct = RepeatAction(request,
                        () => true,
                        () =>
                        {
                            if (IsTacticalReloadEnabled() && IsManualReloadEnabled())
                            {
                                SetTacticalReloadEnabled(false);
                                SetManualReloadEnabled(false);
                                reloadsWereDisabled = true;
                            }
                            Connector.SendMessage($"{request.DisplayViewer} is emptying Snake's weapon clip for {emptyClipDuration.TotalSeconds} seconds.");
                            return true;
                        },
                        TimeSpan.Zero,
                        () => IsReady(request),
                        TimeSpan.FromMilliseconds(100),
                        () =>
                        {
                            Weapon currentWeapon = GetCurrentEquippedWeapon();
                            if (currentWeapon != null && currentWeapon.HasClip)
                            {
                                EmptySnakeClip();
                                return true;
                            }
                            return false;
                        },
                        TimeSpan.FromMilliseconds(100), false);

                    emptyClipAct.WhenCompleted.Then
                        (_ =>
                        {
                            if (reloadsWereDisabled)
                            {
                                SetTacticalReloadEnabled(true);
                                SetManualReloadEnabled(true);
                            }
                            Connector.SendMessage("Emptying Snake's weapon clip effect has ended. Reloads re-enabled.");
                        });

                    break;
                }

            case "setSnakeCurrentWeaponToNone":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                var unequipSnakeWeapon = request.Duration;
                var unequipSnakeWeaponAct = RepeatAction(request,
                    () => true,
                    () => Connector.SendMessage($"{request.DisplayViewer} unequipped Snake's current weapon for {unequipSnakeWeapon.TotalSeconds} seconds."),
                    TimeSpan.Zero,
                    () => IsReady(request),
                    TimeSpan.FromMilliseconds(500),
                () =>
                    {
                        SetSnakeCurrentWeaponToNone();
                        return true;
                    },
                    TimeSpan.FromMilliseconds(500),
                false);
                unequipSnakeWeaponAct.WhenCompleted.Then
                    (_ =>
                {
                    Connector.SendMessage("Snake's weapon has been re-equipped.");
                });
                break;

            case "removeCurrentSuppressor":
                {
                    Weapon currentWeapon = GetCurrentEquippedWeapon();
                    if (IsInCutscene())
                    {
                        DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                        return;
                    }

                    if (currentWeapon != MetalGearSolidDeltaUsableObjects.M1911A1 &&
                        currentWeapon != MetalGearSolidDeltaUsableObjects.MK22 &&
                        currentWeapon != MetalGearSolidDeltaUsableObjects.XM16E1)
                    {
                        Respond(request, EffectStatus.FailTemporary, StandardErrors.PrerequisiteNotFound, "A weapon with a suppressor");
                        return;
                    }

                    RepeatAction(
                        request,
                        () => true,
                        () => true,
                        TimeSpan.Zero,
                        () => IsReady(request),
                        TimeSpan.FromMilliseconds(100),
                        () =>
                        {
                            Weapon weapon = GetCurrentEquippedWeapon();
                            if (weapon is { HasSuppressor: true } &&
                                (weapon == MetalGearSolidDeltaUsableObjects.M1911A1 ||
                                 weapon == MetalGearSolidDeltaUsableObjects.MK22 ||
                                 weapon == MetalGearSolidDeltaUsableObjects.XM16E1))
                            {
                                AddressChain suppressorAddress = weapon.GetPropertyAddress(baseWeaponAddress, WeaponAddresses.SuppressorToggleOffset);
                                // Force the suppressor off.
                                Set8(suppressorAddress, 0);
                            }
                            return true;
                        },
                        TimeSpan.FromMilliseconds(100),
                        false
                    ).WhenCompleted.Then(_ =>
                    {
                        Connector.SendMessage("Suppressors can be equipped again.");
                    });

                    break;
                }

            case "realTimeItemSwap":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                var realTimeSwapDuration = request.Duration;
                var realTimeSwapAct = RepeatAction(request,
                    () => true,
                    () => Connector.SendMessage($"{request.DisplayViewer} stopped Snake from stopping time for {realTimeSwapDuration.TotalSeconds} seconds."),
                    TimeSpan.Zero,
                    // Ignore menu check for this effect since it uses the same address but Pause/Radio will still pause the effect time
                    () => IsReady(request, true),
                    TimeSpan.FromMilliseconds(500),
                    () =>
                    {
                        CheckAndUnpauseGame();
                        return true;
                    },
                    TimeSpan.FromMilliseconds(500),
                    false);
                realTimeSwapAct.WhenCompleted.Then
                (_ =>
                {
                    Connector.SendMessage("Snake can now stop time again.");
                });
                break;

            case "everyoneIsDrunk":
                var drunkDuration = request.Duration;
                var drunkAct = RepeatAction(request,
                    () => true,
                    () => Connector.SendMessage($"{request.DisplayViewer} made everyone drunk for {drunkDuration.TotalSeconds} seconds."),
                    TimeSpan.Zero,
                    () => IsReady(request),
                    TimeSpan.FromMilliseconds(500),
                    () =>
                    {
                        SetDrunkModeEnabled(true);
                        return true;
                    },
                    TimeSpan.FromMilliseconds(500),
                    false);
                drunkAct.WhenCompleted.Then
                (_ =>
                {
                    SetDrunkModeEnabled(false);
                    Connector.SendMessage("Everyone is sober again.");
                });
                break;

            #endregion

            #region Snake's Stats

            case "setSnakeStamina":
                {
                    if (IsInCutscene())
                    {
                        DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                        return;
                    }
                    TryEffect(request,
                        () => true,
                        () =>
                        {
                            SetSnakeStamina();
                            return true;
                        },
                        () => Connector.SendMessage($"{request.DisplayViewer} set Snake's stamina to 0."));
                    break;
                }

            case "setSnakeMaxStamina":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SetSnakeMaxStamina();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} set Snake's Stamina to 30000."));
                break;

            case "setSnakeHealthZero":
                TryEffect(request,
                    () => true,
                    () => { SetSnakeHealthToZero(); return true; },
                    () => Connector.SendMessage($"{request.DisplayViewer} set Snake's health to 0."));
                break;

            case "fillSnakeHealth":
                TryEffect(request,
                    () => true,
                    () => { FillSnakeHealth(); return true; },
                    () => Connector.SendMessage($"{request.DisplayViewer} filled Snake's health to maximum."));
                break;

            case "increaseSnakeMaxHealth":
                TryEffect(request,
                    () => true,
                    () => IncreaseSnakeMaxHealth((short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} increased Snake's maximum health by {5 * request.Quantity}."));
                break;

            case "decreaseSnakeMaxHealth":
                TryEffect(request,
                    () => true,
                    () => DecreaseSnakeMaxHealth((short)request.Quantity),
                    () => Connector.SendMessage($"{request.DisplayViewer} decreased Snake's maximum health by {5 * request.Quantity}."));
                break;
            case "snakeHasTheCommonCold":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SnakeHasTheCommonCold();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake the common cold."));
                break;

            case "snakeIsPoisoned":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SnakeIsPoisoned();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} poisoned Snake."));
                break;

            case "snakeHasFoodPoisoning":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SnakeHasFoodPoisoning();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake food poisoning."));
                break;

            case "snakeHasLeeches":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SnakeHasLeeches();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake leeches."));
                break;

            case "snakeHasABulletBee":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SnakeHasABulletBee();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake a bullet bee."));
                break;

            case "snakeHasATranqDart":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SnakeHasATranqDart();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake a tranq dart."));
                break;

            case "setSnakeCamoIndexNegative":
                if (IsInCutscene() || !IsCamoIndexInstructionNormal())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                var camoIndexNegativeDuration = request.Duration;
                var camoIndexNegativeAct = RepeatAction(request,
                    () => true,
                    () => Connector.SendMessage($"{request.DisplayViewer} set Snake's camo index to -1000 for {camoIndexNegativeDuration.TotalSeconds} seconds."),
                    TimeSpan.Zero,
                    () => IsReady(request),
                    TimeSpan.FromMilliseconds(500),
                    () =>
                    {
                        SetSnakeCamoIndexInstructionToWritable();
                        SetSnakeCamoIndexValue(-1000);
                        return true;
                    },
                    TimeSpan.FromMilliseconds(500),
                    false);
                camoIndexNegativeAct.WhenCompleted.Then
                    (_ =>
                {
                    SetSnakeCamoIndexValue(0);
                    SetSnakeCamoIndexInstructionToNormal();
                    Connector.SendMessage("Snake's camo index is back to normal.");
                });
                break;

            case "setSnakeCamoIndexPositive":
                if (IsInCutscene() || !IsCamoIndexInstructionNormal())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                var camoIndexPositiveDuration = request.Duration;
                var camoIndexPositiveAct = RepeatAction(request,
                    () => true,
                    () => Connector.SendMessage($"{request.DisplayViewer} set Snake's camo index to 1000 for {camoIndexPositiveDuration.TotalSeconds} seconds."),
                    TimeSpan.Zero,
                    () => IsReady(request),
                    TimeSpan.FromMilliseconds(500),
                    () =>
                    {
                        SetSnakeCamoIndexInstructionToWritable();
                        SetSnakeCamoIndexValue(1000);
                        return true;
                    },
                    TimeSpan.FromMilliseconds(500),
                    false);
                camoIndexPositiveAct.WhenCompleted.Then
                    (_ =>
                {
                    SetSnakeCamoIndexValue(0);
                    SetSnakeCamoIndexInstructionToNormal();
                    Connector.SendMessage("Snake's camo index is back to normal.");
                });
                break;

            #endregion

            #region Alert Status

            case "setAlertStatus":
                {
                    if (IsInCutscene() || !IsAlertAllowedOnCurrentMap())
                    {
                        DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                        return;
                    }

                    TryEffect(request,
                        () => true,
                        () =>
                        {
                            SetAlertStatus();
                            return true;
                        },
                        () => Connector.SendMessage($"{request.DisplayViewer} set the game to Alert Status."));
                    break;
                }

            case "setEvasionStatus":
                if (IsInCutscene() || !IsAlertAllowedOnCurrentMap())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SetCautionStatus();
                        /* This 5 seconds gives time for reinforcements to be called which 
                         makes for a better evasion status of guards searching for Snake */
                        Task.Delay(5000).Wait();
                        SetEvasionStatus();
                        Task.Delay(1000).Wait();
                        SetAlertStatus();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} set the game to Evasion Status."));
                break;

            case "setCautionStatus":
                if (IsInCutscene() || !IsAlertAllowedOnCurrentMap())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SetCautionStatus();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} set the game to Caution Status."));
                break;

            #endregion

            #region Filters

            case "removeFog":
                if (IsInCutscene() || !IsFogEnabled())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () => { SetFogEnabled(false); return true; },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed the fog."));
                break;

            case "restoreFog":
                if (IsInCutscene() || IsFogEnabled())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () => { SetFogEnabled(true); return true; },
                    () => Connector.SendMessage($"{request.DisplayViewer} restored the fog."));
                break;

            case "setPissFilter":
                {

                    TryEffect(request,
                        () => true,
                        () =>
                        {
                            SetToPissFilterMode();
                            return true;
                        },
                        () => Connector.SendMessage($"{request.DisplayViewer} made everyone look at the game through piss."));
                    break;
                }

            #endregion

            #region Items

            case "giveLifeMedicine":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, true);
                    return;
                }

                AddressChain currentAddress = MetalGearSolidDeltaUsableObjects.LifeMedicine.GetPropertyAddress(baseItemAddress, ItemAddresses.CurrentCapacityOffset);
                AddressChain maxAddress = MetalGearSolidDeltaUsableObjects.LifeMedicine.GetPropertyAddress(baseItemAddress, ItemAddresses.MaxCapacityOffset);

                Log.Message($"Life Medicine Current Address: {currentAddress}");
                Log.Message($"Life Medicine Max Address: {maxAddress}");

                short currentValue = GetItemValue(MetalGearSolidDeltaUsableObjects.LifeMedicine);
                short maxCapacity = GetItemMaxCapacity(MetalGearSolidDeltaUsableObjects.LifeMedicine);
                Log.Message($"Life Medicine - Current: {currentValue}, Max: {maxCapacity}");

                if ((currentValue + request.Quantity) > maxCapacity)
                {
                    Log.Message("Cannot give Life Medicine - would exceed max capacity");
                    DelayEffect(request, "Snake cannot carry that many Life Medicine.", false);
                    return;
                }

                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short newValue = (short)(currentValue + request.Quantity);
                        Log.Message($"Attempting to set Life Medicine from {currentValue} to {newValue}");
                        SetItemValue(MetalGearSolidDeltaUsableObjects.LifeMedicine, newValue);

                        // Verify the write was successful
                        short verifyValue = GetItemValue(MetalGearSolidDeltaUsableObjects.LifeMedicine);
                        Log.Message($"After setting - Value: {verifyValue}");

                        return verifyValue == newValue;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake {request.Quantity} life med(s). Snake now has {GetItemValue(MetalGearSolidDeltaUsableObjects.LifeMedicine)} life med(s)."));
                break;

            case "giveScope":
                if (IsInCutscene() || (GetItemValue(MetalGearSolidDeltaUsableObjects.Binoculars) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MetalGearSolidDeltaUsableObjects.Binoculars);
                        SetItemValue(MetalGearSolidDeltaUsableObjects.Binoculars, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake a scope."));
                break;

            case "removeScope":
                if (IsInCutscene() || (GetItemValue(MetalGearSolidDeltaUsableObjects.Binoculars) == 0) || (GetItemValue(MetalGearSolidDeltaUsableObjects.Binoculars) == -1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MetalGearSolidDeltaUsableObjects.Binoculars);
                        SetItemValue(MetalGearSolidDeltaUsableObjects.Binoculars, (short)(currentValue = 0));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed a cigar from Snake, guess he is quitting smoking early."));
                break;

            case "giveThermalGoggles":
                if (IsInCutscene() || (GetItemValue(MetalGearSolidDeltaUsableObjects.ThermalGoggles) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MetalGearSolidDeltaUsableObjects.ThermalGoggles);
                        SetItemValue(MetalGearSolidDeltaUsableObjects.ThermalGoggles, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake thermal goggles."));
                break;

            case "removeThermalGoggles":
                if (IsInCutscene() || (GetItemValue(MetalGearSolidDeltaUsableObjects.ThermalGoggles) == 0) || (GetItemValue(MetalGearSolidDeltaUsableObjects.ThermalGoggles) == -1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MetalGearSolidDeltaUsableObjects.ThermalGoggles);
                        SetItemValue(MetalGearSolidDeltaUsableObjects.ThermalGoggles, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed thermal goggles from Snake."));
                break;

            case "giveNightVisionGoggles":
                if (IsInCutscene() || (GetItemValue(MetalGearSolidDeltaUsableObjects.NightVisionGoggles) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MetalGearSolidDeltaUsableObjects.NightVisionGoggles);
                        SetItemValue(MetalGearSolidDeltaUsableObjects.NightVisionGoggles, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake night vision goggles."));
                break;

            case "removeNightVisionGoggles":
                if (IsInCutscene() || (GetItemValue(MetalGearSolidDeltaUsableObjects.NightVisionGoggles) == 0) || (GetItemValue(MetalGearSolidDeltaUsableObjects.NightVisionGoggles) == -1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MetalGearSolidDeltaUsableObjects.NightVisionGoggles);
                        SetItemValue(MetalGearSolidDeltaUsableObjects.NightVisionGoggles, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed night vision goggles from Snake."));
                break;

            case "giveMotionDetector":
                if (IsInCutscene() || (GetItemValue(MetalGearSolidDeltaUsableObjects.MotionDetector) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MetalGearSolidDeltaUsableObjects.MotionDetector);
                        SetItemValue(MetalGearSolidDeltaUsableObjects.MotionDetector, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake a motion detector."));
                break;

            case "removeMotionDetector":
                if (IsInCutscene() || (GetItemValue(MetalGearSolidDeltaUsableObjects.MotionDetector) == 0) || (GetItemValue(MetalGearSolidDeltaUsableObjects.MotionDetector) == -1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MetalGearSolidDeltaUsableObjects.MotionDetector);
                        SetItemValue(MetalGearSolidDeltaUsableObjects.MotionDetector, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed a motion detector from Snake."));
                break;

            case "giveSonar":
                if (IsInCutscene() || (GetItemValue(MetalGearSolidDeltaUsableObjects.ActiveSonar) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MetalGearSolidDeltaUsableObjects.ActiveSonar);
                        SetItemValue(MetalGearSolidDeltaUsableObjects.ActiveSonar, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake a sonar."));
                break;

            case "removeSonar":
                if (IsInCutscene() || (GetItemValue(MetalGearSolidDeltaUsableObjects.ActiveSonar) == 0) || (GetItemValue(MetalGearSolidDeltaUsableObjects.ActiveSonar) == -1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MetalGearSolidDeltaUsableObjects.ActiveSonar);
                        SetItemValue(MetalGearSolidDeltaUsableObjects.ActiveSonar, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed a sonar from Snake."));
                break;

            case "giveAntiPersonnelSensor":
                if (IsInCutscene() || (GetItemValue(MetalGearSolidDeltaUsableObjects.AntiPersonnelSensor) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MetalGearSolidDeltaUsableObjects.AntiPersonnelSensor);
                        SetItemValue(MetalGearSolidDeltaUsableObjects.AntiPersonnelSensor, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake an anti-personnel sensor."));
                break;

            case "removeAntiPersonnelSensor":
                if (IsInCutscene() || (GetItemValue(MetalGearSolidDeltaUsableObjects.AntiPersonnelSensor) == 0) || (GetItemValue(MetalGearSolidDeltaUsableObjects.AntiPersonnelSensor) == -1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MetalGearSolidDeltaUsableObjects.AntiPersonnelSensor);
                        SetItemValue(MetalGearSolidDeltaUsableObjects.AntiPersonnelSensor, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed an anti-personnel sensor from Snake."));
                break;

            case "giveAntidote":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemMaxCapacity(MetalGearSolidDeltaUsableObjects.Antidote) <= GetItemValue(MetalGearSolidDeltaUsableObjects.Antidote)))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MetalGearSolidDeltaUsableObjects.Antidote);
                        short maxCapacity = GetItemMaxCapacity(MetalGearSolidDeltaUsableObjects.Antidote);

                        // Prevent exceeding max capacity
                        if ((currentValue + request.Quantity) > maxCapacity)
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.Antidote, maxCapacity);
                        }
                        else
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.Antidote, (short)(currentValue + request.Quantity));
                        }
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake {request.Quantity} antidote(s). Snake now has {GetItemValue(MetalGearSolidDeltaUsableObjects.Antidote)} antidote(s)."));
                break;

            case "removeAntidote":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemValue(MetalGearSolidDeltaUsableObjects.Antidote) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MetalGearSolidDeltaUsableObjects.Antidote);

                        // Prevent going below 0
                        if ((currentValue - request.Quantity) < 0)
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.Antidote, 0);
                        }
                        else
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.Antidote, (short)(currentValue - request.Quantity));
                        }
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed {request.Quantity} antidote(s) from Snake, he now has {GetItemValue(MetalGearSolidDeltaUsableObjects.Antidote)} antidote(s)."));
                break;

            case "giveCMed":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemMaxCapacity(MetalGearSolidDeltaUsableObjects.ColdMedicine) <= GetItemValue(MetalGearSolidDeltaUsableObjects.ColdMedicine)))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MetalGearSolidDeltaUsableObjects.ColdMedicine);
                        short maxCapacity = GetItemMaxCapacity(MetalGearSolidDeltaUsableObjects.ColdMedicine);

                        if ((currentValue + request.Quantity) > maxCapacity)
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.ColdMedicine, maxCapacity);
                        }
                        else
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.ColdMedicine, (short)(currentValue + request.Quantity));
                        }
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake {request.Quantity} C Med(s). Snake now has {GetItemValue(MetalGearSolidDeltaUsableObjects.ColdMedicine)} C Med(s)."));
                break;

            case "removeCMed":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemValue(MetalGearSolidDeltaUsableObjects.ColdMedicine) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MetalGearSolidDeltaUsableObjects.ColdMedicine);

                        if ((currentValue - request.Quantity) < 0)
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.ColdMedicine, 0);
                        }
                        else
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.ColdMedicine, (short)(currentValue - request.Quantity));
                        }
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed {request.Quantity} C Med(s) from Snake, he now has {GetItemValue(MetalGearSolidDeltaUsableObjects.ColdMedicine)} C Med(s)."));
                break;


            case "giveDMed":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemMaxCapacity(MetalGearSolidDeltaUsableObjects.DigestiveMedicine) <= GetItemValue(MetalGearSolidDeltaUsableObjects.DigestiveMedicine)))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MetalGearSolidDeltaUsableObjects.DigestiveMedicine);
                        short maxCapacity = GetItemMaxCapacity(MetalGearSolidDeltaUsableObjects.DigestiveMedicine);

                        if ((currentValue + request.Quantity) > maxCapacity)
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.DigestiveMedicine, maxCapacity);
                        }
                        else
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.DigestiveMedicine, (short)(currentValue + request.Quantity));
                        }
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake {request.Quantity} D Med(s). Snake now has {GetItemValue(MetalGearSolidDeltaUsableObjects.DigestiveMedicine)} D Med(s)."));
                break;

            case "removeDMed":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemValue(MetalGearSolidDeltaUsableObjects.DigestiveMedicine) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MetalGearSolidDeltaUsableObjects.DigestiveMedicine);

                        if ((currentValue - request.Quantity) < 0)
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.DigestiveMedicine, 0);
                        }
                        else
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.DigestiveMedicine, (short)(currentValue - request.Quantity));
                        }
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed {request.Quantity} D Med(s) from Snake, he now has {GetItemValue(MetalGearSolidDeltaUsableObjects.DigestiveMedicine)} D Med(s)."));
                break;

            case "giveSerum":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemMaxCapacity(MetalGearSolidDeltaUsableObjects.Serum) <= GetItemValue(MetalGearSolidDeltaUsableObjects.Serum)))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MetalGearSolidDeltaUsableObjects.Serum);
                        short maxCapacity = GetItemMaxCapacity(MetalGearSolidDeltaUsableObjects.Serum);

                        if ((currentValue + request.Quantity) > maxCapacity)
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.Serum, maxCapacity);
                        }
                        else
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.Serum, (short)(currentValue + request.Quantity));
                        }
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake {request.Quantity} serum(s). Snake now has {GetItemValue(MetalGearSolidDeltaUsableObjects.Serum)} serum(s)."));
                break;

            case "removeSerum":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemValue(MetalGearSolidDeltaUsableObjects.Serum) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MetalGearSolidDeltaUsableObjects.Serum);

                        if ((currentValue - request.Quantity) < 0)
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.Serum, 0);
                        }
                        else
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.Serum, (short)(currentValue - request.Quantity));
                        }
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed {request.Quantity} serum(s) from Snake, he now has {GetItemValue(MetalGearSolidDeltaUsableObjects.Serum)} serum(s)."));
                break;

            case "giveBandage":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemMaxCapacity(MetalGearSolidDeltaUsableObjects.Bandage) <= GetItemValue(MetalGearSolidDeltaUsableObjects.Bandage)))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MetalGearSolidDeltaUsableObjects.Bandage);
                        short maxCapacity = GetItemMaxCapacity(MetalGearSolidDeltaUsableObjects.Bandage);

                        if ((currentValue + request.Quantity) > maxCapacity)
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.Bandage, maxCapacity);
                        }
                        else
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.Bandage, (short)(currentValue + request.Quantity));
                        }
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake {request.Quantity} bandage(s). Snake now has {GetItemValue(MetalGearSolidDeltaUsableObjects.Bandage)} bandage(s)."));
                break;

            case "removeBandage":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemValue(MetalGearSolidDeltaUsableObjects.Bandage) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MetalGearSolidDeltaUsableObjects.Bandage);

                        if ((currentValue - request.Quantity) < 0)
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.Bandage, 0);
                        }
                        else
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.Bandage, (short)(currentValue - request.Quantity));
                        }
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed {request.Quantity} bandage(s) from Snake, he now has {GetItemValue(MetalGearSolidDeltaUsableObjects.Bandage)} bandage(s)."));
                break;

            case "giveDisinfectant":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemMaxCapacity(MetalGearSolidDeltaUsableObjects.Disinfectant) <= GetItemValue(MetalGearSolidDeltaUsableObjects.Disinfectant)))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MetalGearSolidDeltaUsableObjects.Disinfectant);
                        short maxCapacity = GetItemMaxCapacity(MetalGearSolidDeltaUsableObjects.Disinfectant);

                        if ((currentValue + request.Quantity) > maxCapacity)
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.Disinfectant, maxCapacity);
                        }
                        else
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.Disinfectant, (short)(currentValue + request.Quantity));
                        }
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake {request.Quantity} disinfectant(s). Snake now has {GetItemValue(MetalGearSolidDeltaUsableObjects.Disinfectant)} disinfectant(s)."));
                break;

            case "removeDisinfectant":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemValue(MetalGearSolidDeltaUsableObjects.Disinfectant) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MetalGearSolidDeltaUsableObjects.Disinfectant);

                        if ((currentValue - request.Quantity) < 0)
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.Disinfectant, 0);
                        }
                        else
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.Disinfectant, (short)(currentValue - request.Quantity));
                        }
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed {request.Quantity} disinfectant(s) from Snake, he now has {GetItemValue(MetalGearSolidDeltaUsableObjects.Disinfectant)} disinfectant(s)."));
                break;

            case "giveOintment":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemMaxCapacity(MetalGearSolidDeltaUsableObjects.Ointment) <= GetItemValue(MetalGearSolidDeltaUsableObjects.Ointment)))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MetalGearSolidDeltaUsableObjects.Ointment);
                        short maxCapacity = GetItemMaxCapacity(MetalGearSolidDeltaUsableObjects.Ointment);

                        if ((currentValue + request.Quantity) > maxCapacity)
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.Ointment, maxCapacity);
                        }
                        else
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.Ointment, (short)(currentValue + request.Quantity));
                        }
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake {request.Quantity} ointment(s). Snake now has {GetItemValue(MetalGearSolidDeltaUsableObjects.Ointment)} ointment(s)."));
                break;

            case "removeOintment":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemValue(MetalGearSolidDeltaUsableObjects.Ointment) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MetalGearSolidDeltaUsableObjects.Ointment);

                        if ((currentValue - request.Quantity) < 0)
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.Ointment, 0);
                        }
                        else
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.Ointment, (short)(currentValue - request.Quantity));
                        }
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed {request.Quantity} ointment(s) from Snake, he now has {GetItemValue(MetalGearSolidDeltaUsableObjects.Ointment)} ointment(s)."));
                break;

            case "giveSplint":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemMaxCapacity(MetalGearSolidDeltaUsableObjects.Splint) <= GetItemValue(MetalGearSolidDeltaUsableObjects.Splint)))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MetalGearSolidDeltaUsableObjects.Splint);
                        short maxCapacity = GetItemMaxCapacity(MetalGearSolidDeltaUsableObjects.Splint);

                        if ((currentValue + request.Quantity) > maxCapacity)
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.Splint, maxCapacity);
                        }
                        else
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.Splint, (short)(currentValue + request.Quantity));
                        }
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake {request.Quantity} splint(s). Snake now has {GetItemValue(MetalGearSolidDeltaUsableObjects.Splint)} splint(s)."));
                break;

            case "removeSplint":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemValue(MetalGearSolidDeltaUsableObjects.Splint) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MetalGearSolidDeltaUsableObjects.Splint);

                        if ((currentValue - request.Quantity) < 0)
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.Splint, 0);
                        }
                        else
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.Splint, (short)(currentValue - request.Quantity));
                        }
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed {request.Quantity} splint(s) from Snake, he now has {GetItemValue(MetalGearSolidDeltaUsableObjects.Splint)} splint(s)."));
                break;

            case "giveStyptic":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemMaxCapacity(MetalGearSolidDeltaUsableObjects.Styptic) <= GetItemValue(MetalGearSolidDeltaUsableObjects.Styptic)))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MetalGearSolidDeltaUsableObjects.Styptic);
                        short maxCapacity = GetItemMaxCapacity(MetalGearSolidDeltaUsableObjects.Styptic);

                        if ((currentValue + request.Quantity) > maxCapacity)
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.Styptic, maxCapacity);
                        }
                        else
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.Styptic, (short)(currentValue + request.Quantity));
                        }
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake {request.Quantity} styptic(s). Snake now has {GetItemValue(MetalGearSolidDeltaUsableObjects.Styptic)} styptic(s)."));
                break;

            case "removeStyptic":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemValue(MetalGearSolidDeltaUsableObjects.Styptic) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MetalGearSolidDeltaUsableObjects.Styptic);

                        if ((currentValue - request.Quantity) < 0)
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.Styptic, 0);
                        }
                        else
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.Styptic, (short)(currentValue - request.Quantity));
                        }
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed {request.Quantity} styptic(s) from Snake, he now has {GetItemValue(MetalGearSolidDeltaUsableObjects.Styptic)} styptic(s)."));
                break;

            case "giveSutureKit":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemMaxCapacity(MetalGearSolidDeltaUsableObjects.SutureKit) <= GetItemValue(MetalGearSolidDeltaUsableObjects.SutureKit)))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MetalGearSolidDeltaUsableObjects.SutureKit);
                        short maxCapacity = GetItemMaxCapacity(MetalGearSolidDeltaUsableObjects.SutureKit);

                        if ((currentValue + request.Quantity) > maxCapacity)
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.SutureKit, maxCapacity);
                        }
                        else
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.SutureKit, (short)(currentValue + request.Quantity));
                        }
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake {request.Quantity} suture kit(s). Snake now has {GetItemValue(MetalGearSolidDeltaUsableObjects.SutureKit)} suture kit(s)."));
                break;

            case "removeSutureKit":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemValue(MetalGearSolidDeltaUsableObjects.SutureKit) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MetalGearSolidDeltaUsableObjects.SutureKit);

                        if ((currentValue - request.Quantity) < 0)
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.SutureKit, 0);
                        }
                        else
                        {
                            SetItemValue(MetalGearSolidDeltaUsableObjects.SutureKit, (short)(currentValue - request.Quantity));
                        }
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed {request.Quantity} suture kit(s) from Snake, he now has {GetItemValue(MetalGearSolidDeltaUsableObjects.SutureKit)} suture kit(s)."));
                break;

            #endregion

            #region Damage to Snake Multipliers

            case "setSnakeDamageX2":
                if (IsInCutscene() || !CanSetSnakeDamageMultiplier())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                var damageX2Duration = request.Duration;
                var damageX2Act = RepeatAction(request,
                    () => true,
                    () => Connector.SendMessage($"{request.DisplayViewer} set Snake's damage multiplier to x2 for {damageX2Duration.TotalSeconds} seconds."),
                    TimeSpan.Zero,
                    () => IsReady(request),
                    TimeSpan.FromMilliseconds(500),
                    () =>
                    {
                        SetSnakeDamageMultiplierInstruction();
                        SetSnakeDamageMultiplierValue(2);
                        return true;
                    },
                    TimeSpan.FromMilliseconds(500),
                    false);
                damageX2Act.WhenCompleted.Then
                    (_ =>
                {
                    SetSnakeDamageMultiplierValue(1);
                    Connector.SendMessage("Snake's damage multiplier is back to x1.");
                });
                break;

            case "setSnakeDamageX3":
                if (IsInCutscene() || !CanSetSnakeDamageMultiplier())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                var damageX3Duration = request.Duration;
                var damageX3Act = RepeatAction(request,
                    () => true,
                    () => Connector.SendMessage($"{request.DisplayViewer} set Snake's damage multiplier to x3 for {damageX3Duration.TotalSeconds} seconds."),
                    TimeSpan.Zero,
                    () => IsReady(request),
                    TimeSpan.FromMilliseconds(500),
                    () =>
                    {
                        SetSnakeDamageMultiplierInstruction();
                        SetSnakeDamageMultiplierValue(3);
                        return true;
                    },
                    TimeSpan.FromMilliseconds(500),
                    false);
                damageX3Act.WhenCompleted.Then
                    (_ =>
                {
                    SetSnakeDamageMultiplierValue(1);
                    Connector.SendMessage("Snake's damage multiplier is back to x1.");
                });
                break;

            case "setSnakeDamageX4":
                if (IsInCutscene() || !CanSetSnakeDamageMultiplier())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                var damageX4Duration = request.Duration;
                var damageX4Act = RepeatAction(request,
                    () => true,
                    () => Connector.SendMessage($"{request.DisplayViewer} set Snake's damage multiplier to x4 for {damageX4Duration.TotalSeconds} seconds."),
                    TimeSpan.Zero,
                    () => IsReady(request),
                    TimeSpan.FromMilliseconds(500),
                    () =>
                    {
                        SetSnakeDamageMultiplierInstruction();
                        SetSnakeDamageMultiplierValue(4);
                        return true;
                    },
                    TimeSpan.FromMilliseconds(500),
                    false);
                damageX4Act.WhenCompleted.Then
                    (_ =>
                {
                    SetSnakeDamageMultiplierValue(1);
                    Connector.SendMessage("Snake's damage multiplier is back to x1.");
                });
                break;

            case "setSnakeDamageX5":
                if (IsInCutscene() || !CanSetSnakeDamageMultiplier())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                var damageX5Duration = request.Duration;
                var damageX5Act = RepeatAction(request,
                    () => true,
                    () => Connector.SendMessage($"{request.DisplayViewer} set Snake's damage multiplier to x5 for {damageX5Duration.TotalSeconds} seconds."),
                    TimeSpan.Zero,
                    () => IsReady(request),
                    TimeSpan.FromMilliseconds(500),
                    () =>
                    {
                        SetSnakeDamageMultiplierInstruction();
                        SetSnakeDamageMultiplierValue(5);
                        return true;
                    },
                    TimeSpan.FromMilliseconds(500),
                    false);
                damageX5Act.WhenCompleted.Then
                    (_ =>
                {
                    SetSnakeDamageMultiplierValue(1);
                    Connector.SendMessage("Snake's damage multiplier is back to x1.");
                });
                break;

            #endregion

            #region Snake's Stats and Active Equipment

            case "MakeSnakeTrip":
                if (IsInCutscene() || !IsSleepAllowedOnCurrentMap())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        MakeSnakeTrip();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} made Snake quick sleep."));
                break;

            case "MakeSnakeSleep":
                TryEffect(request,
                        () => true,
                        () =>
                        {
                            MakeSnakeSleep();
                            return true;
                        },
                        () => Connector.SendMessage($"{request.DisplayViewer} made Snake sleep for 20 seconds."));
                break;

            case "makeSnakePukeFire":

                TryEffect(request,
                    () => true,
                    () =>
                    {
                        MakeSnakePukeFire();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} made Snake puke fire."));
                break;

            case "makeSnakePuke":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        MakeSnakePuke();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} made Snake puke."));
                break;

            case "setSnakeOnFire":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SetSnakeOnFire();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} set Snake on fire."));
                break;

            #endregion

            #region Guard Stats

            case "setGuardStatsAlmostInvincible":
                var almostInvincibleDuration = request.Duration;

                var almostInvincibleAct = RepeatAction(request,
                    () => true,
                    () => Connector.SendMessage($"{request.DisplayViewer} set the guards to be almost invincible for {almostInvincibleDuration.TotalSeconds} seconds."),
                    TimeSpan.Zero,
                    () => IsReady(request),
                    TimeSpan.FromMilliseconds(500),
                    () =>
                    {
                        SetGuardLethalDamageInvincible();
                        SetGuardSleepDamageAlmostInvincible();
                        SetGuardStunAlmostDamageInvincible();
                        return true;
                    },
                    TimeSpan.FromMilliseconds(500),
                    false);

                almostInvincibleAct.WhenCompleted.Then
                (_ =>
                {
                    SetGuardLethalDamageDefault();
                    SetGuardSleepDamageDefault();
                    SetGuardStunDamageDefault();
                    Connector.SendMessage("Guard stats are back to default.");
                });

                break;

            case "setGuardStatsVeryStrong":
                var veryStrongDuration = request.Duration;

                var veryStrongAct = RepeatAction(request,
                    () => true,
                    () => Connector.SendMessage($"{request.DisplayViewer} set the guards to be very strong for {veryStrongDuration.TotalSeconds} seconds."),
                    TimeSpan.Zero,
                    () => IsReady(request),
                    TimeSpan.FromMilliseconds(500),
                    () =>
                    {
                        SetGuardLethalDamageVeryStrong();
                        SetGuardSleepDamageVeryStrong();
                        SetGuardStunDamageVeryStrong();
                        return true;
                    },
                    TimeSpan.FromMilliseconds(500),
                    false);
                veryStrongAct.WhenCompleted.Then
                (_ =>
                {
                    SetGuardLethalDamageDefault();
                    SetGuardSleepDamageDefault();
                    SetGuardStunDamageDefault();
                    Connector.SendMessage("Guard stats are back to default.");
                });

                break;

            case "setGuardStatsVeryWeak":
                var veryWeakDuration = request.Duration;

                var veryWeakAct = RepeatAction(request,
                    () => true,
                    () => Connector.SendMessage($"{request.DisplayViewer} set the guards to be very weak for {veryWeakDuration.TotalSeconds} seconds."),
                    TimeSpan.Zero,
                    () => IsReady(request),
                    TimeSpan.FromMilliseconds(500),
                    () =>
                    {
                        SetGuardLethalDamageVeryWeak();
                        SetGuardSleepDamageVeryWeak();
                        SetGuardStunDamageVeryWeak();
                        return true;
                    },
                    TimeSpan.FromMilliseconds(500),
                    false);
                veryWeakAct.WhenCompleted.Then
                (_ =>
                {
                    SetGuardLethalDamageDefault();
                    SetGuardSleepDamageDefault();
                    SetGuardStunDamageDefault();
                    Connector.SendMessage("Guard stats are back to default.");
                });
                break;

            case "setGuardStatsOneShot":
                var oneShotDuration = request.Duration;

                var oneShotAct = RepeatAction(request,
                    () => true,
                    () => Connector.SendMessage($"{request.DisplayViewer} set the guards to be one shot for {oneShotDuration.TotalSeconds} seconds."),
                    TimeSpan.Zero,
                    () => IsReady(request),
                    TimeSpan.FromMilliseconds(500),
                    () =>
                    {
                        SetGuardLethalDamageOneshot();
                        SetGuardSleepDamageOneshot();
                        SetGuardStunDamageOneshot();
                        return true;
                    },
                    TimeSpan.FromMilliseconds(500),
                    false);
                oneShotAct.WhenCompleted.Then
                (_ =>
                {
                    SetGuardLethalDamageDefault();
                    SetGuardSleepDamageDefault();
                    SetGuardStunDamageDefault();
                    Connector.SendMessage("Guard stats are back to default.");
                });
                break;

            default:
                Respond(request, EffectStatus.FailPermanent, StandardErrors.UnknownEffect, request);
                break;

                #endregion
        }
    }
}

#endregion