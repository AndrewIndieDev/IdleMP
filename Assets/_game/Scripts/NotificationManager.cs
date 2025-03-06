using UnityEngine;

public class NotificationManager : MonoBehaviour
{
    private static NotificationManager Instance;
    private void Awake() => Instance = this;

    [SerializeField] private UINotification notificationPrefab;
    [SerializeField] private Transform notificationContent;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            int number = Random.Range(0, 1000);
            Add("Title" + number, "Message " + number);
        }
    }

    public static void Add(string title, string message)
    {
        UINotification notification = Instantiate(Instance.notificationPrefab, Instance.notificationContent);
        notification.UpdateText(title, message);
    }
}
