using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using JamesFrowen.SimpleWeb;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Xsolla.Catalog;
using Xsolla.Core;
using Xsolla.GameKeys;
using Xsolla.Inventory;
using Xsolla.UserAccount;

public class HangarManager : MonoBehaviour
{

    XsollaPlayerDataManager xsollaPlayerDataManager;
    PopupManager popupManager;

    public Button buyButton;
    public Button couponButton;
    public Button leftHangarButton;
    public Button rightHangarButton;
    public Button selectSkinButton;


    public GameObject positionSelectedSkin;
    public GameObject positionNotSelectedSkin;
    public GameObject freighterHangar;
    public GameObject strikerHangar;
    public GameObject hunterHangar;
    public GameObject spectreHangar;

    public GameObject lockedImage;
    public GameObject unlockedImage;

    private GameObject[] hangarShips;
    private int currentSkinIndex;

    public TMP_InputField couponInput;
    public TMP_Text currentSkinText;
    public TMP_Text selectedSkinText;
    public TMP_Text killsText;
    public TMP_Text usernameText; 
    public TMP_Text selectedFlagHangarText; 



    [Header("SETTINGS")]
    public Button settingsButton;
    public GameObject settingsUI;
    public GameObject flagButtonPrefab;
    public Button leftFlagButton;
    public Button rightFlagButton;
    public Button selectFlagButton;
    public TMP_Text chosenFlagText; 

    public Image chosenFlagImage;
    private List<string> countryKeys;
    private int currentFlagIndex;

    
    private protected bool isSkinOwned { get; set; }
    private List<string> ownedSkus = new List<string>();

    [System.Serializable]
    public class HangarShipData
    {
        public string skinName;
        public string sku; // Input this into Xsolla, Find storeItem.sku, return storeItem.sku, for unlock.
        public bool is_free;
    }

    private void Start()
    {
        xsollaPlayerDataManager = FindFirstObjectByType<XsollaPlayerDataManager>();
        popupManager = FindFirstObjectByType<PopupManager>();

        // Initialize hangarShips array and selected index
        hangarShips = new GameObject[] { freighterHangar, strikerHangar, hunterHangar, spectreHangar};
        currentSkinIndex = 0;

        countryKeys = new List<string>(CountryDatabase.countryCodes.Keys);
        currentFlagIndex = 0;
        UpdateFlagImage();


        // string email; 
        // email = XsollaUserAccount.GetUserEmail();

        // Set initial positions
        UpdateHangarPositions();
        UpdateHangarText();


        selectFlagButton.onClick.AddListener(ProcessSelectFlag); // Activate Login Menu
        settingsButton.onClick.AddListener(EnableSettingsUI); // Activate Login Menu
        leftFlagButton.onClick.AddListener(ProcessLeftFlagButton); //
        rightFlagButton.onClick.AddListener(ProcessRightFlagButton); // Activate Login Menu

        buyButton.onClick.AddListener(ProcessBuy); // Activate Login Menu
        couponButton.onClick.AddListener(OpenCouponInput); // Activate Login Menu
        selectSkinButton.onClick.AddListener(SelectSkin); // Activate Login Menu
        leftHangarButton.onClick.AddListener(CycleLeftHangar); // Activate Login Menu
        rightHangarButton.onClick.AddListener(CycleRightHangar); // De-Activate Login Menu
    }

    private void ProcessSelectFlag()
    {
        if (xsollaPlayerDataManager.isLoggedIn != true) { popupManager.ShowPopup("login required!", true); return; }
        string selectedFlag = chosenFlagText.text;
        xsollaPlayerDataManager.SelectFlag(selectedFlag);
        UpdateHangarText();
    }

    private void ProcessLeftFlagButton()
    {
        currentFlagIndex = (currentFlagIndex - 1 + countryKeys.Count) % countryKeys.Count;
        UpdateFlagImage();
    }


    private void ProcessRightFlagButton()
    {
        currentFlagIndex = (currentFlagIndex + 1) % countryKeys.Count;
        UpdateFlagImage();
    }

    private void UpdateFlagImage()
    {
        string countryCode = countryKeys[currentFlagIndex]; // Get the code of the index
        string countryName = CountryDatabase.countryCodes[countryCode]; // get the country name of the index
        chosenFlagText.text = countryName;

        string flagPath = countryName.ToLower().Replace(" ", "_"); // set the path correct to the file
        flagPath = $"Flags/{flagPath}_16"; // dito

        Sprite flagSprite = Resources.Load<Sprite>(flagPath); // Create a flagSprite variable.
        chosenFlagImage.sprite = flagSprite; // Set the flagsprite

        // Debug.Log($"Selected flag: {countryName} ({countryCode})");
    }


    private void EnableSettingsUI()
    {
        settingsUI.gameObject.SetActive(!settingsUI.gameObject.activeSelf);
    }

    public void LoadHangarBackend() // CALLED ON SUCCESS LOGIN
    {
        XsollaCatalog.GetCatalog(OnItemsRequestSuccess, OnError); // Fetch all items for sale on store
		XsollaInventory.GetInventoryItems(OnItemsRequestSuccess, OnError); // Fetch all items in inventory
    }

    private void OnError(Error error)
    {
        Debug.LogError($"Error: {error.errorMessage}");
        popupManager.ShowPopup("error!", true);
    }


    /////////////////////
    // PLAYERINVENTORY //
    /////////////////////
    private void OnItemsRequestSuccess(InventoryItems inventoryItems) // Fetch players items
    {
        // Iterating the items collection
        foreach (var inventoryItem in inventoryItems.items)
        {
            // Skipping virtual currency items
            if (inventoryItem.VirtualItemType == VirtualItemType.VirtualCurrency)
                continue;

            //Debug.Log("All Sku's Owned: " + inventoryItem.sku);
            ownedSkus.Add(inventoryItem.sku);
        }
        UpdateHangarText();
    }

    ///////////
    // STORE //
    ///////////
    private void OnItemsRequestSuccess(StoreItems storeItems) // Fetch store items for sale
    {
        // Iterating the items collection
        foreach (var storeItem in storeItems.items)
        {
            // Skipping items without prices in real money
            if (storeItem.price == null)
                continue;

            // Debug.Log(storeItem.sku);
            
            // Debug.Log(storeItem);
        }
    }

    private void CycleLeftHangar()
    {
        // Debug.Log("CycleLeft");
        currentSkinIndex = (currentSkinIndex - 1 + hangarShips.Length) % hangarShips.Length;
        UpdateHangarPositions();
        UpdateHangarText();
    }

    private void CycleRightHangar()
    {
        // popupManager.ShowPopup("Hello, this is a debug message!");
        // Debug.Log("CycleRight");
        currentSkinIndex = (currentSkinIndex + 1) % hangarShips.Length;
        UpdateHangarPositions();
        UpdateHangarText();
    }

    private void UpdateHangarPositions()
    {
        // Set the selected hangarShip to the selected position
        hangarShips[currentSkinIndex].transform.position = positionSelectedSkin.transform.position;

        // Set the next hangarShipp to the not selected position
        int nextIndex = (currentSkinIndex + 1) % hangarShips.Length;
        hangarShips[nextIndex].transform.position = positionNotSelectedSkin.transform.position;

        // Set the previous hangarShip to the not selected position
        int prevIndex = (currentSkinIndex - 1 + hangarShips.Length) % hangarShips.Length;
        hangarShips[prevIndex].transform.position = positionNotSelectedSkin.transform.position;
    }

    private void UpdateHangarText()
    {
        HangarShip selectedShip = hangarShips[currentSkinIndex].GetComponent<HangarShip>();
        currentSkinText.text = selectedShip.shipData.skinName;

        // Check if the selected ship's SKU is in the database of owned SKUs
        bool isItemOwned = ownedSkus.Contains(selectedShip.shipData.sku);
        
        if (isItemOwned)
        {
            // Item is unlocked
            // selectSkinButton.interactable = true; 
            isSkinOwned = true;
            lockedImage.SetActive(false);
            unlockedImage.SetActive(true);
            buyButton.gameObject.SetActive(false);
        }

        else
        {
            // Item is locked
            // selectSkinButton.interactable = false; 
            isSkinOwned = false;
            lockedImage.SetActive(true);
            unlockedImage.SetActive(false);
            buyButton.gameObject.SetActive(true);
        }

        //  if ( xsollaPlayerDataManager._playerData.Name != null ) // Was buggy, boolion set instead...
        if (xsollaPlayerDataManager.isLoggedIn == true)
        {
            killsText.text = "kills: " + xsollaPlayerDataManager._playerData.Kills.ToString();
            usernameText.text = "user: " + xsollaPlayerDataManager.displayName.ToString();
            selectedSkinText.text = "ship: " + xsollaPlayerDataManager._playerData.SelectedSkin.ToString();
            selectedFlagHangarText.text = "flag: " + xsollaPlayerDataManager._playerData.Country.ToString();
        }
    }

    public void ClearHangarText()
    {
        killsText.text = "kills: " + " ";
        usernameText.text = "user: " + " ";
        selectedSkinText.text = "ship: " + " ";
        selectedFlagHangarText.text = "flag: " + " ";
    }

    public void ChangeShipData(int index, string newSkinName)
    {
        HangarShip ship = hangarShips[index].GetComponent<HangarShip>();
        ship.shipData.skinName = newSkinName;
    }

    private void SelectSkin() // Prevent this if skin is locked
    {
        // TODO Xsolla has an IsUserAuthenticated. Switch to this
        if ( xsollaPlayerDataManager._playerData.Name == null ) { popupManager.ShowPopup("login required!", true); return; }
        if ( isSkinOwned == false ) { popupManager.ShowPopup("locked!", true); return;}


        UpdateHangarText();
        HangarShip selectedShip = hangarShips[currentSkinIndex].GetComponent<HangarShip>();
        xsollaPlayerDataManager._playerData.SelectedSkin = selectedShip.shipData.skinName;
        selectedSkinText.text = "ship: " + selectedShip.shipData.skinName.ToString(); 
        xsollaPlayerDataManager.SaveDataXsolla(); // CARE THIS WILL CHANGE PLAYER DATA
        popupManager.ShowPopup("selected ship: " + selectedShip.shipData.skinName.ToString(), false);
    }

    private void ProcessBuy()
    {
        string selectedBuyShipSKU; 
        bool selectedBuyShipIsFree;

        if ( xsollaPlayerDataManager._playerData.Name == null ) { popupManager.ShowPopup("login required!", true); return; }

        HangarShip selectedBuyShip = hangarShips[currentSkinIndex].GetComponent<HangarShip>(); // selected ship

        selectedBuyShipSKU = selectedBuyShip.shipData.sku;
        selectedBuyShipIsFree = selectedBuyShip.shipData.is_free;

        if (selectedBuyShipIsFree == true) 
        { 
            XsollaCatalog.PurchaseFreeItem(selectedBuyShipSKU, OnPurchaseSuccess, OnError);
            return; // stop execution if item is free
        }
        // Debug.Log("selectedBuyShipSKU: " + selectedBuyShipSKU);
        XsollaCatalog.Purchase(selectedBuyShipSKU, OnPurchaseSuccess, OnError);
    }

    private void OnPurchaseSuccess(OrderStatus status)
    {
        Debug.Log("Purchase successful");
        XsollaInventory.GetInventoryItems(OnItemsRequestSuccess, OnError); // Fetch all items in inventory
        popupManager.ShowPopup("payment successful!", false);
    }

    void Update()
    {
        if (couponInput.isActiveAndEnabled)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                ProcessCoupon();
            }
        }
    }

    public void ClearOwnedSkus()
    {
        ownedSkus.Clear();
        UpdateHangarText();
    }

    private void ProcessCoupon()
    {
        string coupon = couponInput.text;

        if ( xsollaPlayerDataManager._playerData.Name == null ) { popupManager.ShowPopup("login required!", true); return; }

        // Call RedeemCouponCode with the required callbacks
        XsollaCatalog.RedeemCouponCode(
            coupon,
            onSuccess: (redeemedItems) => {
                // Handle successful coupon redemption
                UpdateHangarText();
                popupManager.ShowPopup("coupon successfully redeemed!", false);
                Debug.Log("Coupon successfully redeemed!");
                // Add your success handling code here, e.g., updating UI or inventory
            },
            onError: OnError // Use your existing OnError method
        );
    }
    
    private void OpenCouponInput()
    {
        couponInput.gameObject.SetActive(!couponInput.gameObject.activeSelf);

    }
}
