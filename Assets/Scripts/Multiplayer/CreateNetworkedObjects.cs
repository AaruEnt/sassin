using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CreateNetworkedObjects : MonoBehaviour
{
    public List<GameObject> toSpawn = new List<GameObject> ();
    public List<Vector3> positions = new List<Vector3> ();

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < toSpawn.Count; i++)
            {
                PhotonNetwork.Instantiate(toSpawn[i].name, positions[i], Quaternion.identity, 0);
            }
        }
    }
}
