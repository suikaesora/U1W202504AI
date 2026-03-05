using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LicenseButton : MonoBehaviour
{
    [SerializeField]
    private LicenseWindow _licenseWindow;

    public void OnLicenseButton()
    {
        _licenseWindow.Open();
    }
}
