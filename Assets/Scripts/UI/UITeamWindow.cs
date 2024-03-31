using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITeamWindow : UIWindow
{
    public UITeamInfoPage infoPage;
    public UITeamBagPage bagPage;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void switchPage(bool infoOrBag)
    {
        infoPage.gameObject.SetActive(infoOrBag);
        bagPage.gameObject.SetActive(!infoOrBag);
    }
}
