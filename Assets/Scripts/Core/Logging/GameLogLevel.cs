using System;

namespace Core.Logging
{
    /// <summary>
    /// Defined logging levels.
    /// </summary>
    public enum GameLogLevel
    {
        Debug, Info, Warning, Error
    }

    public static class GameLogLevelExtensions
    {
        public static int ToInt(this GameLogLevel gameLogLevel)
        {
            return gameLogLevel switch
            {
                GameLogLevel.Debug => 0,
                GameLogLevel.Info => 1,
                GameLogLevel.Warning => 2,
                GameLogLevel.Error => 3,
                _ => throw new ArgumentOutOfRangeException(nameof(gameLogLevel), gameLogLevel, null)
            };
        }
    }
}