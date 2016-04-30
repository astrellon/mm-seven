using UnityEngine;
using System.Collections;

public class CatGoal : MonoBehaviour {

    private GameObject player;
    private float messageCountdown = 3;

    public TextMesh MessageDisplay;

    private string[] FarMessages = new[]
    {
        "Need cat friends",
        "Want cat friends",
        "I has fish",
        "Cat think about friends"
    };
    private string[] NearMessages = new[]
    {
        "Hi friend bring more friends",
        "This fish mine",
        "There are no other fish"
    };

	// Use this for initialization
	void Start () {
        player = GameObject.FindGameObjectWithTag("Player");
	}

    void ChangeMessage(string message)
    {
        MessageDisplay.text = message;
        messageCountdown = Random.Range(2, 4);
    }

    string GetNextMessage(float distanceToPlayer)
    {
        if (distanceToPlayer < 7)
        {
            return NearMessages[Random.Range(0, NearMessages.Length)];
        }
        return FarMessages[Random.Range(0, FarMessages.Length)];
    }
	
	// Update is called once per frame
	void Update () {
        messageCountdown -= Time.deltaTime;

        var toPlayer = player.transform.position - transform.position;
        var distanceToPlayer = toPlayer.magnitude;
        toPlayer /= distanceToPlayer;

        if (messageCountdown < 0)
        {
            ChangeMessage(GetNextMessage(distanceToPlayer));
        }

        if (distanceToPlayer < 7)
        {
            var cross = Vector3.Cross(toPlayer, transform.forward);
            if (cross.y < -0.1)
            {
                transform.Rotate(0, 180.0f * Time.deltaTime, 0);
            }
            else if (cross.y > 0.1)
            {
                transform.Rotate(0, -180.0f * Time.deltaTime, 0);
            }
        }


    }
}
