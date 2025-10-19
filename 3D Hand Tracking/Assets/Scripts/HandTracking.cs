using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandTracking : MonoBehaviour
{
    public UDPReceive udpReceive;
    public GameObject[] handPoints;
    [SerializeField] private float offset = 8.5f;
    [SerializeField] private float division = 100f;

    // Public property to access hand points
    public GameObject[] HandPoints => handPoints;

    void Update()
    {
        string data = udpReceive.data;

        if (data.Length >= 2 && data.StartsWith("[") && data.EndsWith("]"))
        {
            data = data.Substring(1, data.Length - 2);

            //print(data);

            string[] points = data.Split(',');

            for (int i = 0; i < 21; i++)
            {
                float x = offset - float.Parse(points[i * 3]) / division; //div by 100 to make the python values smaller for unity
                float y = float.Parse(points[i * 3 + 1]) / division;
                float z = float.Parse(points[i * 3 + 2]) / division;

                handPoints[i].transform.localPosition = new Vector3(x, y, z);
            }
        }
    }
}
