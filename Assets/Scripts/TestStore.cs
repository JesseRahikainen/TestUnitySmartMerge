using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class TestStore : MonoBehaviour, IStoreListener {

    private static IStoreController _storeController;
    private static IExtensionProvider _storeExtensionProvider;

    public const string productID = "testProductID";

    // Use this for initialization
    void Start( )
    {
		if( _storeController == null ) {
            InitializePurchasing( );
        }
	}

    public void InitializePurchasing( )
    {
        if( IsInitialized( ) ) {
            Debug.Log( "Already initialized" );
            return;
        }

        var builder = ConfigurationBuilder.Instance( StandardPurchasingModule.Instance( ) );

        // add a product to sell / restore by way of it's identifier
        builder.AddProduct( productID, ProductType.Consumable );

        UnityPurchasing.Initialize( this, builder );
    }

    public bool IsInitialized( )
    {
        return ( ( _storeController != null ) && ( _storeExtensionProvider != null ) );
    }

    public void BuyTestProduct( )
    {
        BuyProductID( productID );
    }

    public void BuyProductID( string id )
    {
        if( !IsInitialized( ) ) {
            MessageDialogs.ShowMessageDialog(
                "Store Error",
                "Store is not yet initialized.",
                "OK", null );
            return;
        }

        Product product = _storeController.products.WithID( productID );

        if( product == null ) {
            MessageDialogs.ShowMessageDialog(
                "Store Error",
                "Problem retrieving product.",
                "OK", null );
            return;
        }

        // starting purchase, is async so will need some sort of loading pop-up
        _storeController.InitiatePurchase( product );
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnInitialized( IStoreController controller, IExtensionProvider extensions )
    {
        Debug.Log( "Store initialized" );
        _storeController = controller;
        _storeExtensionProvider = extensions;
    }

    public void OnInitializeFailed( InitializationFailureReason error )
    {
        MessageDialogs.ShowMessageDialog(
            "Store Error",
            "Problem initializing:\n" + error,
            "OK", null );
    }

    public void OnPurchaseFailed( Product i, PurchaseFailureReason p )
    {
        MessageDialogs.ShowMessageDialog(
            "Store Error",
            "Failed to purchase product " + i.definition.storeSpecificId + ":\n" + p,
            "OK", null );
    }

    public PurchaseProcessingResult ProcessPurchase( PurchaseEventArgs e )
    {
        if( string.Equals( e.purchasedProduct.definition.id, productID, StringComparison.Ordinal ) ) {
            MessageDialogs.ShowMessageDialog(
                "Purchase Complete",
                "Thanks for your money!",
                "OK", null );
        }

        return PurchaseProcessingResult.Complete;
    }
}
