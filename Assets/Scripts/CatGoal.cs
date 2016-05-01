using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CatGoal : MonoBehaviour {

    private GameObject player;
    private float messageCountdown = 3;
    public HashSet<GameObject> CatsAtGoal = new HashSet<GameObject>();
    private float changeTargetCountdown = 3.0f;
    private GameObject target;
    public int MaxFriends = 5;
    public GameObject Confetti;
    private bool doneGame = false;

    private float winTimer = 0;

    public TextMesh MessageDisplay;

    private string[] FarMessages = new[]
    {
        "Need cat friends",
        "Want cat friends",
        "I has fish",
        "Cat think about friends"
    };
    private string[] FarSomeFriendsMessages = new[]
    {
        "Play with friends!",
        "Maybe I share fish?",
        "We need some milk up in here!"
    };
    private string[] NearMessages = new[]
    {
        "Hi friend bring more friends",
        "This fish mine",
        "There are no other fish"
    };
    private string[] NearSomeFriendsMessages = new[]
    {
        "You brought freinds!",
        "You are friend too!",
        "Maybe tummy rubs sometime!"
    };
    private string[] MaxFriendMessages = new[]
    {
        "Has all the friends!",
        "I love pussy!",
        "All the fuzz!",
        "All the purr",
        "So much meow!",
        "Too much mew?",
        "Like mew!"
    };

    private Dictionary<int, string> Numbers = new Dictionary<int, string>()
    {
        { 2, "two" },
        { 3, "three" },
        { 4, "four" },
        { 5, "five" },
        { 6, "six" },
        { 7, "seven" },
        { 8, "eight" },
        { 9, "nine" },
        { 10, "ten" },
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
        var numCats = CatsAtGoal.Count;
        if (numCats >= MaxFriends)
        {
            return GetRandomMessage(MaxFriendMessages);
        }

        if (numCats > 0 && Random.value > 0.7f)
        {
            if (CatsAtGoal.Count == 1)
            {
                return "Yay one new friend!";
            }
            else if (CatsAtGoal.Count > 10)
            {
                return "So many friends!";
            }
            return "Has " + Numbers[CatsAtGoal.Count] + " cats here!";
        }

        if (distanceToPlayer < 7)
        {
            return GetRandomMessage(numCats > 0 ? NearSomeFriendsMessages : NearMessages);
        }
        return GetRandomMessage(numCats > 0 ? FarSomeFriendsMessages : FarMessages);
    }
    string GetRandomMessage(string[] messages)
    {
        return messages[Random.Range(0, messages.Length)];
    }

    GameObject GetRandomTarget(float distanceToPlayer)
    {
        var targets = new List<GameObject>();
        if (distanceToPlayer < 7)
        {
            targets.Add(player);
        }
        targets.AddRange(CatsAtGoal);

        if (targets.Count == 0)
        {
            return null;
        }

        return targets[Random.Range(0, targets.Count)];
    }

    void OnGUI()
    {
        if (!doneGame)
        {
            return;
        }
        var percentage = winTimer / 3.0f;
        if (percentage > 1.0f)
        {
            percentage = 1.0f;
        }

        percentage *= 0.4f;

        var screenPos1 = new Vector2(Screen.width * 0.5f - 200, Screen.height * percentage - 40);
        var screenPos2 = new Vector2(Screen.width * 0.5f - 200, Screen.height * (1 - percentage) + 40);
        var style = GUI.skin.GetStyle("Label");
        style.alignment = TextAnchor.UpperCenter;
        style.fontSize = 30;
        var mod = Mathf.RoundToInt(winTimer) % 3; 
        if (mod == 0)
        {
            style.normal.textColor = Color.red;
        }
        else if (mod == 1)
        {
            style.normal.textColor = Color.white;
        }
        else
        {
            style.normal.textColor = Color.black;
        }
        GUI.Label(new Rect(screenPos1, new Vector2(400, 40)), "You Win Cats!", style);
        GUI.Label(new Rect(screenPos2, new Vector2(400, 40)), "Happy 7 years!", style);
    }
	
	// Update is called once per frame
	void Update ()
    {
        messageCountdown -= Time.deltaTime;

        changeTargetCountdown -= Time.deltaTime;

        var toPlayer = player.transform.position - transform.position;
        var distanceToPlayer = toPlayer.magnitude;

        if (changeTargetCountdown < 0)
        {
            changeTargetCountdown = Random.Range(1.0f, 2.0f);
            target = GetRandomTarget(distanceToPlayer);
        }

        if (messageCountdown < 0)
        {
            ChangeMessage(GetNextMessage(distanceToPlayer));
        }

        if (doneGame)
        {
            winTimer += Time.deltaTime;
        }

        if (CatsAtGoal.Count >= MaxFriends)
        {
            if (!doneGame)
            {
                doneGame = true;
                Confetti.SetActive(true);
            }
            transform.Rotate(0, 180.0f * Time.deltaTime, 0);
        }
        else if (target != null)
        {
            var toTarget = target.transform.position - transform.position;
            var distanceToTarget = toTarget.magnitude;
            toTarget /= distanceToTarget;

            var cross = Vector3.Cross(toTarget, transform.forward);
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
