using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFiveElements : MonoBehaviour
{
    public FiveElementsPropertyDisplay propertyDisplay;
    
    public void Setup(FiveElements elementsData)
    {
        if (!elementsData.isValid)
        {
            gameObject.SetActive(false);
        }
        else
        {
            propertyDisplay.UpdateElements(elementsData);
            gameObject.SetActive(true);
        }
    }
}
