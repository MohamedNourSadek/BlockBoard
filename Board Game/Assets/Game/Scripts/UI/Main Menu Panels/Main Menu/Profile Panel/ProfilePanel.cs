using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfilePanel : Panel
{
    public override void Show()
    {
        base.Show();

        if (Manager.GetManager<PlayfabManager>().IsLoggedIn)
            Panel.GetPanel<UserInfoPanel>().Show();
        else
            Panel.GetPanel<ProfileNoLoginPanel>().Show();
    }

}
