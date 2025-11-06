using UnityEngine;
using AsteroidMiner.Core;

namespace AsteroidMiner.Entities
{
    /// <summary>
    /// Tracks and manages player statistics throughout the game session.
    /// Works in conjunction with GameState to update statistical data.
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameState gameState;
        
        // Session statistics (not saved, reset each play session)
        private float sessionStartTime;
        private int sessionCreditsEarned = 0;
        private int sessionAsteroidsMined = 0;
        private float sessionDistanceTraveled = 0f;
        
        // Cached values for change detection
        private int lastCredits = 0;
        private int lastAsteroidsMined = 0;
        private float lastDistanceTraveled = 0f;
        
        // Properties
        public float SessionPlayTime => Time.time - sessionStartTime;
        public int SessionCreditsEarned => sessionCreditsEarned;
        public int SessionAsteroidsMined => sessionAsteroidsMined;
        public float SessionDistanceTraveled => sessionDistanceTraveled;
        
        #region Unity Lifecycle
        
        private void Start()
        {
            sessionStartTime = Time.time;
            
            // Get GameState from GameManager (will implement later)
            if (gameState == null)
            {
                Debug.LogWarning("PlayerStats: No GameState assigned.");
            }
            else
            {
                // Initialize cached values
                lastCredits = gameState.credits;
                lastAsteroidsMined = gameState.asteroidsMined;
                lastDistanceTraveled = gameState.distanceTraveled;
            }
        }
        
        private void Update()
        {
            if (gameState == null) return;
            
            // Update total play time
            gameState.totalPlayTime += Time.deltaTime;
            
            // Track session changes
            TrackSessionChanges();
        }
        
        #endregion
        
        #region Statistics Tracking
        
        /// <summary>
        /// Track changes in statistics during this session.
        /// </summary>
        private void TrackSessionChanges()
        {
            // Track credits earned this session
            if (gameState.credits > lastCredits)
            {
                sessionCreditsEarned += (gameState.credits - lastCredits);
                lastCredits = gameState.credits;
            }
            
            // Track asteroids mined this session
            if (gameState.asteroidsMined > lastAsteroidsMined)
            {
                sessionAsteroidsMined = gameState.asteroidsMined - lastAsteroidsMined;
            }
            
            // Track distance traveled this session
            sessionDistanceTraveled = gameState.distanceTraveled - lastDistanceTraveled;
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Set the game state reference.
        /// </summary>
        public void SetGameState(GameState state)
        {
            gameState = state;
            
            if (gameState != null)
            {
                lastCredits = gameState.credits;
                lastAsteroidsMined = gameState.asteroidsMined;
                lastDistanceTraveled = gameState.distanceTraveled;
            }
        }
        
        /// <summary>
        /// Record an asteroid being mined.
        /// </summary>
        public void RecordAsteroidMined()
        {
            if (gameState != null)
            {
                gameState.asteroidsMined++;
            }
        }
        
        /// <summary>
        /// Record a hazard being destroyed.
        /// </summary>
        public void RecordHazardDestroyed()
        {
            if (gameState != null)
            {
                gameState.hazardsDestroyed++;
            }
        }
        
        /// <summary>
        /// Record a sector being explored.
        /// </summary>
        public void RecordSectorExplored()
        {
            if (gameState != null)
            {
                gameState.sectorsExplored++;
            }
        }
        
        /// <summary>
        /// Record a mission being completed.
        /// </summary>
        public void RecordMissionCompleted()
        {
            if (gameState != null)
            {
                gameState.missionsCompleted++;
            }
        }
        
        /// <summary>
        /// Get a summary of current statistics.
        /// </summary>
        public string GetStatsSummary()
        {
            if (gameState == null) return "No GameState";
            
            return $"Credits: {gameState.credits}\n" +
                   $"Asteroids Mined: {gameState.asteroidsMined}\n" +
                   $"Distance Traveled: {gameState.distanceTraveled:F2}\n" +
                   $"Hazards Destroyed: {gameState.hazardsDestroyed}\n" +
                   $"Sectors Explored: {gameState.sectorsExplored}\n" +
                   $"Missions Completed: {gameState.missionsCompleted}\n" +
                   $"Play Time: {FormatPlayTime(gameState.totalPlayTime)}\n" +
                   $"Session Time: {FormatPlayTime(SessionPlayTime)}";
        }
        
        /// <summary>
        /// Format play time in hours:minutes:seconds.
        /// </summary>
        private string FormatPlayTime(float seconds)
        {
            int hours = Mathf.FloorToInt(seconds / 3600f);
            int minutes = Mathf.FloorToInt((seconds % 3600f) / 60f);
            int secs = Mathf.FloorToInt(seconds % 60f);
            
            if (hours > 0)
                return $"{hours}h {minutes}m {secs}s";
            else if (minutes > 0)
                return $"{minutes}m {secs}s";
            else
                return $"{secs}s";
        }
        
        #endregion
    }
}
