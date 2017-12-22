using System;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmDialog : MonoBehaviour
{
    public Text titleText;
    public Text questionText;
    public Button cancelButton;
    public Button confirmButton;
    public Action confirmAction;
    public Action cancelAction;

    public void Start( )
    {
        cancelButton.onClick.AddListener( OnCancelPressed );
        confirmButton.onClick.AddListener( OnConfirmPressed );
    }

    public void Show(string title, string question, string confirmBtn, Action confirmAction, string cancelBtn = null, Action cancelAction = null)
    {
        titleText.text = title;
        questionText.text = question;
        confirmButton.GetComponentInChildren<Text>().text = confirmBtn;
        cancelButton.GetComponentInChildren<Text>( ).text = cancelBtn;
        this.confirmAction = confirmAction;
        this.cancelAction = cancelAction;

        gameObject.SetActive( true );
    }

    public void OnCancelPressed()
    {
        if( cancelAction != null ) cancelAction.Invoke( );
        gameObject.SetActive(false);
    }

    public void OnConfirmPressed()
    {
        if( confirmAction != null ) confirmAction.Invoke();
        gameObject.SetActive(false);
    }
}
