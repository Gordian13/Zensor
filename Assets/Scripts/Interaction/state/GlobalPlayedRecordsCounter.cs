using UnityEngine;

public class GlobalPlayedRecordsCounter : GameIntValueSource
{
    public static GlobalPlayedRecordsCounter Instance { get; private set; }

    [SerializeField] private int numRecordsPlayed;

    public override int CurrentValue => numRecordsPlayed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        numRecordsPlayed = 0;
    }

    public void IncreaseRecordsPlayed()
    {
        numRecordsPlayed++;

        Debug.Log($"Player played a record. Count: {numRecordsPlayed}");

        NotifyValueChanged(numRecordsPlayed);
    }

    public void ResetRecordsPlayed()
    {
        numRecordsPlayed = 0;

        Debug.Log("Reset records played to 0.");

        NotifyValueChanged(numRecordsPlayed);
    }
}