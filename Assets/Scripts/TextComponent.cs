using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using TMPro;

public class TextComponent : MonoBehaviour
{
    TMP_Text text;

    int cont = 0;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TMP_Text>();
    }

    public void addObjective()
    {
        cont++;
        text.text = "Objetivos acertados: " + cont.ToString();
    }
}
