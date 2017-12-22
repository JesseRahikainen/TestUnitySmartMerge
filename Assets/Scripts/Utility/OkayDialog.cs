using System;
using UnityEngine;
using UnityEngine.UI;

public class OkayDialog : MonoBehaviour
{
    public Text titleText;
    public Text noticeText;
    public Button okayButton;
    public Action okayAction;

    public void Start( )
    {
        okayButton.onClick.AddListener( OnOkayPressed );
    }

    public void Show( string title, string notice, string okBtn = "Ok", Action okayAction = null )
    {
        titleText.text = title;
        noticeText.text = notice;
        okayButton.GetComponentInChildren<Text>( ).text = okBtn;
        this.okayAction = okayAction;
        gameObject.SetActive( true );
    }

    public void OnOkayPressed()
    {
        if( okayAction != null ) okayAction.Invoke( );
        gameObject.SetActive(false);
    }
}
