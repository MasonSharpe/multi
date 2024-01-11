using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class PlayerNetwork : NetworkBehaviour
{
    public Transform spawnedObjectPrefab;
    public Slider slider;
    public Transform knife;

    private NetworkVariable<Color> color = new(Color.white, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<int> health = new(10, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<float> knifeTimer = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        if (OwnerClientId == 1)
        {
            
            transform.position += Vector3.right * 2;
        }
        else
        {
            transform.position -= Vector3.right * 2;
        }

        if (OwnerClientId == 1)
        {
            knife.parent.transform.rotation = Quaternion.Euler(0, 0, 180);
        }

        base.OnNetworkSpawn();
    }
    private void Update()
    {
        GetComponentInChildren<SpriteRenderer>().color = color.Value;
        slider.value = health.Value / 10f;
        knife.localPosition = new Vector3(Mathf.Sin(knifeTimer.Value * 3 + OwnerClientId * 100f) * 3 + 1, 0, 0);



        if (!IsOwner) return;

        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        GetComponent<Rigidbody2D>().velocity = new Vector2(9 * x,  9 * y);

        if (Input.GetKeyDown(KeyCode.E))
        {
            color.Value = new Color(Random.Range(0f, 1), Random.Range(0f, 1), Random.Range(0f, 1));


        }

        knifeTimer.Value += Time.deltaTime;


    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsOwner) return;
        print(name);
        if (collision.gameObject.layer == 6)
        {
            health.Value--;
        }
    }
}
