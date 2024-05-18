using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class InputCorrectionManager : Manager
{
    public static InputCorrection GetCorrectionMessage(string input, InputType inputType)
    {
        switch (inputType)
        {
            case InputType.Email:
                return GetEmailCorrection(input);
                break;
            case InputType.Username:
                return GetUsernameCorrection(input);
                break;
            case InputType.Password:
                return GetPasswordCorrection(input);
                break;
            default:
                return new InputCorrection { IsInputCorrect = true, CorrectionMessage = "" };
        }
    }

    public static InputCorrection GetEmailCorrection(string email)
    {
        InputCorrection correction = new InputCorrection();

        if (email.Length >= 5)
        {
            correction.CorrectionMessage = "";
            correction.IsInputCorrect = true;
        }
        else if(email.Length == 0)
        {
            correction.CorrectionMessage = "";
            correction.IsInputCorrect = false;
        }
        else
        {
            correction.CorrectionMessage = "Your email should be 5 letters or more";
            correction.IsInputCorrect = false;
        }

        return correction;
    }
    public static InputCorrection GetUsernameCorrection(string username)
    {
        InputCorrection correction = new InputCorrection();

        if (username.Length >= 4)
        {
            correction.CorrectionMessage = "";
            correction.IsInputCorrect = true;
        }
        else if (username.Length == 0)
        {
            correction.CorrectionMessage = "";
            correction.IsInputCorrect = false;
        }
        else
        {
            correction.CorrectionMessage = "Your username should be 4 letters or more";
            correction.IsInputCorrect = false;
        }

        return correction;
    }
    public static InputCorrection GetPasswordCorrection(string password)
    {
        InputCorrection correction = new InputCorrection();

        if (password.Length >= 5)
        {
            correction.CorrectionMessage = "";
            correction.IsInputCorrect = true;
        }
        else if (password.Length == 0)
        {
            correction.CorrectionMessage = "";
            correction.IsInputCorrect = false;
        }
        else
        {
            correction.CorrectionMessage = "Your password should be 5 letters or more";
            correction.IsInputCorrect = false;
        }

        return correction;
    }

}

public struct InputCorrection
{
    public bool IsInputCorrect;
    public string CorrectionMessage;
}

public enum InputType
{
    Email,
    Username,
    Password
}
