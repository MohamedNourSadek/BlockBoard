using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfilePanel : Panel
{
    public override void Show()
    {
        base.Show();

        if(Manager.GetManager<PlayfabManager>().IsLoggedIn)
            Show<UserInfoPanel>();
        else 
            Show<ProfileNoLoginPanel>();
    }

}
