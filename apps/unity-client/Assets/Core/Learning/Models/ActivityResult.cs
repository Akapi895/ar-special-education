using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Learning.Models
{
    /// <summary>
    /// Represents the result of an activity completion.
    /// </summary>
    [Serializable]
    public class ActivityResult
    {
        /// <summary>
        /// Unique identifier for the activity type (e.g., "QuantityMatch", "CompareQuantity").
        /// </summary>
        public string ActivityId;

        /// <summary>
        /// Session ID to group results from the same play session.
        /// </summary>
        public string SessionId;

        /// <summary>
        /// Learner profile ID this result belongs to.
        /// </summary>
        public string LearnerId;

        /// <summary>
        /// The level or question number within the activity.
        /// </summary>
        public int LevelNumber;

        /// <summary>
        /// Difficulty level of this activity/level.
        /// </summary>
        public DifficultyLevel DifficultyLevel;

        /// <summary>
        /// Whether the final answer was correct.
        /// </summary>
        public bool IsCorrect;

        /// <summary>
        /// Total number of attempts made.
        /// </summary>
        public int TotalAttempts;

        /// <summary>
        /// Number of hints used during the activity.
        /// </summary>
        public int HintsUsedCount;

        /// <summary>
        /// Total time spent on the activity (in seconds).
        /// </summary>
        public float TimeSpentSeconds;

        /// <summary>
        /// Timestamp when the activity was started.
        /// </summary>
        public DateTime StartTime;

        /// <summary>
        /// Timestamp when the activity was completed.
        /// </summary>
        public DateTime EndTime;

        /// <summary>
        /// ISO 8601 string representation of StartTime for JSON serialization.
        /// JsonUtility doesn't serialize DateTime directly.
        /// </summary>
        [SerializeField]
        private string startTimeString;

        /// <summary>
        /// ISO 8601 string representation of EndTime for JSON serialization.
        /// JsonUtility doesn't serialize DateTime directly.
        /// </summary>
        [SerializeField]
        private string endTimeString;

        [SerializeField]
        private LearningIssueRecord learningIssue;

        [SerializeField]
        private TechnicalIssueRecord technicalIssue;

        [SerializeField]
        private List<string> skillTags = new List<string>();

        /// <summary>
        /// Public access to the serialized start time string.
        /// </summary>
        public string StartTimeString
        {
            get => startTimeString;
            set
            {
                startTimeString = value;
                if (DateTime.TryParse(value, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime dt))
                {
                    StartTime = dt;
                }
            }
        }

        /// <summary>
        /// Public access to the serialized end time string.
        /// </summary>
        public string EndTimeString
        {
            get => endTimeString;
            set
            {
                endTimeString = value;
                if (DateTime.TryParse(value, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime dt))
                {
                    EndTime = dt;
                }
            }
        }

        /// <summary>
        /// Lesson identifier this round belongs to.
        /// </summary>
        public string LessonId;

        /// <summary>
        /// Stable round identifier for dashboards and exports.
        /// </summary>
        public string RoundId;

        /// <summary>
        /// False when this record represents a platform/AR issue instead of a learning answer.
        /// </summary>
        public bool CountsTowardMastery = true;

        /// <summary>
        /// The type of learning error made (if incorrect).
        /// Backed by a serializable structure because Unity JsonUtility cannot persist nullable enums.
        /// </summary>
        public ErrorType? ErrorType
        {
            get => learningIssue.HasError ? learningIssue.ErrorType : (ErrorType?)null;
            set => learningIssue = LearningIssueRecord.From(value);
        }

        /// <summary>
        /// Serializable learning issue details for analytics/export.
        /// </summary>
        public LearningIssueRecord LearningIssue
        {
            get => learningIssue;
            set => learningIssue = value;
        }

        /// <summary>
        /// Serializable technical issue details. Technical issues do not affect mastery.
        /// </summary>
        public TechnicalIssueRecord TechnicalIssue
        {
            get => technicalIssue;
            set
            {
                technicalIssue = value;
                if (technicalIssue.HasIssue)
                {
                    CountsTowardMastery = false;
                }
            }
        }

        public IReadOnlyList<string> SkillTags => skillTags;
        public bool HasTechnicalIssue => technicalIssue.HasIssue;

        /// <summary>
        /// Optional additional data for specific activities.
        /// </summary>
        public string AdditionalData;

        public ActivityResult()
        {
        }

        public ActivityResult(string activityId, string sessionId, int levelNumber, DifficultyLevel difficultyLevel)
        {
            ActivityId = activityId;
            SessionId = sessionId;
            LevelNumber = levelNumber;
            DifficultyLevel = difficultyLevel;
            StartTime = DateTime.UtcNow;
            RoundId = $"{activityId}-R{levelNumber:00}";
        }

        /// <summary>
        /// Marks the activity as completed with the given result.
        /// </summary>
        public void Complete(bool isCorrect, ErrorType? errorType = null)
        {
            IsCorrect = isCorrect;
            ErrorType = errorType;
            EndTime = DateTime.UtcNow;
            TimeSpentSeconds = (float)(EndTime - StartTime).TotalSeconds;
        }

        public void SetLessonContext(string lessonId, IEnumerable<string> tags)
        {
            LessonId = lessonId;

            skillTags.Clear();
            if (tags == null)
            {
                return;
            }

            foreach (string tag in tags)
            {
                if (!string.IsNullOrEmpty(tag) && !skillTags.Contains(tag))
                {
                    skillTags.Add(tag);
                }
            }
        }

        public void SetTechnicalIssue(TechnicalIssueType issueType, string note = null, string source = null)
        {
            TechnicalIssue = TechnicalIssueRecord.Create(issueType, note, source);
        }

        /// <summary>
        /// Increments the attempt counter.
        /// </summary>
        public void IncrementAttempts()
        {
            TotalAttempts++;
        }

        /// <summary>
        /// Increments the hint usage counter.
        /// </summary>
        public void IncrementHintsUsed()
        {
            HintsUsedCount++;
        }
    }

    [Serializable]
    public struct LearningIssueRecord
    {
        public bool HasError;
        public ErrorType ErrorType;
        public string ErrorCode;
        public string Notes;

        public static LearningIssueRecord From(ErrorType? errorType, string notes = null)
        {
            if (!errorType.HasValue)
            {
                return default;
            }

            return new LearningIssueRecord
            {
                HasError = true,
                ErrorType = errorType.Value,
                ErrorCode = errorType.Value.ToString(),
                Notes = notes
            };
        }
    }

    [Serializable]
    public struct TechnicalIssueRecord
    {
        public bool HasIssue;
        public TechnicalIssueType IssueType;
        public string IssueCode;
        public string Note;
        public string Source;
        public string TimestampString;

        public static TechnicalIssueRecord Create(TechnicalIssueType issueType, string note = null, string source = null)
        {
            return new TechnicalIssueRecord
            {
                HasIssue = true,
                IssueType = issueType,
                IssueCode = issueType.ToString(),
                Note = note,
                Source = source,
                TimestampString = DateTime.UtcNow.ToString("o")
            };
        }
    }

    [Serializable]
    public class RoundResult
    {
        public string RoundId;
        public string LessonId;
        public string ActivityId;
        public int RoundNumber;
        public bool IsCorrect;
        public int Attempts;
        public int HintsUsed;
        public float TimeSpentSeconds;
        public LearningIssueRecord LearningIssue;
    }

    [Serializable]
    public class LessonResult
    {
        public string LessonId;
        public string ActivityId;
        public int RoundsCompleted;
        public int CorrectRounds;
        public int TotalHintsUsed;
        public float TotalTimeSpentSeconds;
        public List<string> SkillTags = new List<string>();

        public float Accuracy => RoundsCompleted > 0 ? (float)CorrectRounds / RoundsCompleted : 0f;
    }

    [Serializable]
    public class ActivitySessionResult
    {
        public string SessionId;
        public string ActivityId;
        public string LessonId;
        public string StartTimeString;
        public string EndTimeString;
        public List<RoundResult> Rounds = new List<RoundResult>();
        public List<TechnicalIssueRecord> TechnicalIssues = new List<TechnicalIssueRecord>();
    }

    [Serializable]
    public class SkillMastery
    {
        public string SkillTag;
        public int Attempts;
        public int CorrectAttempts;
        public int HintsUsed;
        public float AverageTimeSeconds;
        public float MasteryScore;

        public bool IsStrong => Attempts >= 3 && MasteryScore >= 0.8f;
        public bool NeedsPractice => Attempts > 0 && MasteryScore < 0.7f;
    }

    [Serializable]
    public class LessonDefinition
    {
        public string LessonId;
        public string Title;
        public string ActivityId;
        public DifficultyLevel Difficulty;
        public int RecommendedAgeMin;
        public int RecommendedAgeMax;
        public string[] SkillTags;
        public string[] PrerequisiteLessonIds;
        public int MinimumRoundsForMastery;
        public float MinimumAccuracyForMastery;

        public LessonDefinition(
            string lessonId,
            string title,
            string activityId,
            DifficultyLevel difficulty,
            string[] skillTags,
            string[] prerequisiteLessonIds = null,
            int recommendedAgeMin = 5,
            int recommendedAgeMax = 8,
            int minimumRoundsForMastery = 3,
            float minimumAccuracyForMastery = 0.7f)
        {
            LessonId = lessonId;
            Title = title;
            ActivityId = activityId;
            Difficulty = difficulty;
            SkillTags = skillTags ?? Array.Empty<string>();
            PrerequisiteLessonIds = prerequisiteLessonIds ?? Array.Empty<string>();
            RecommendedAgeMin = recommendedAgeMin;
            RecommendedAgeMax = recommendedAgeMax;
            MinimumRoundsForMastery = minimumRoundsForMastery;
            MinimumAccuracyForMastery = minimumAccuracyForMastery;
        }
    }

    public static class LessonMapRegistry
    {
        private static readonly LessonDefinition[] lessons =
        {
            new LessonDefinition("L01", "Nhan biet nhanh 1-3", "QuantityMatch", DifficultyLevel.Easy,
                new[] { "Counting", "Subitizing" }),
            new LessonDefinition("L02", "Dem tung vat 1-5", "QuantityMatch", DifficultyLevel.Easy,
                new[] { "Counting", "OneToOneCounting" }, new[] { "L01" }),
            new LessonDefinition("L03", "Ghep so voi luong 1-5", "QuantityMatch", DifficultyLevel.Easy,
                new[] { "Counting", "QuantitySymbolMapping" }, new[] { "L02" }),
            new LessonDefinition("L04", "Ghep so voi luong 6-10", "QuantityMatch", DifficultyLevel.Medium,
                new[] { "Counting", "QuantitySymbolMapping" }, new[] { "L03" }),
            new LessonDefinition("L05", "Nhieu hon / it hon 1-5", "CompareQuantity", DifficultyLevel.Easy,
                new[] { "MoreFewer", "OneToOneCounting" }, new[] { "L03" }),
            new LessonDefinition("L06", "Bang nhau", "CompareQuantity", DifficultyLevel.Easy,
                new[] { "Equality", "OneToOneCounting" }, new[] { "L05" }),
            new LessonDefinition("L07", "Dau lon hon nho hon bang", "CompareQuantity", DifficultyLevel.Medium,
                new[] { "MoreFewer", "Equality" }, new[] { "L06" }),
            new LessonDefinition("L08", "Truc so 0-5", "NumberLineJump", DifficultyLevel.Easy,
                new[] { "NumberOrder" }, new[] { "L07" }),
            new LessonDefinition("L09", "Truc so 0-10", "NumberLineJump", DifficultyLevel.Medium,
                new[] { "NumberOrder" }, new[] { "L08" }),
            new LessonDefinition("L10", "Cong bang buoc nhay", "NumberLineJump", DifficultyLevel.Medium,
                new[] { "AdditionOnNumberLine" }, new[] { "L09" }),
            new LessonDefinition("L11", "Tru bang buoc nhay", "NumberLineJump", DifficultyLevel.Medium,
                new[] { "SubtractionOnNumberLine" }, new[] { "L10" }),
            new LessonDefinition("L12", "On tap tron ky nang", "QuantityMatch", DifficultyLevel.Hard,
                new[] { "Counting", "MoreFewer", "NumberOrder" }, new[] { "L04", "L07", "L11" })
        };

        public static IReadOnlyList<LessonDefinition> AllLessons => lessons;

        public static LessonDefinition GetLesson(string lessonId)
        {
            if (string.IsNullOrEmpty(lessonId))
            {
                return null;
            }

            foreach (LessonDefinition lesson in lessons)
            {
                if (lesson.LessonId == lessonId)
                {
                    return lesson;
                }
            }

            return null;
        }

        public static LessonDefinition GetFirstLessonForActivity(string activityId)
        {
            foreach (LessonDefinition lesson in lessons)
            {
                if (lesson.ActivityId == activityId)
                {
                    return lesson;
                }
            }

            return null;
        }

        public static LessonDefinition GetRecommendedLessonForActivity(string activityId, int roundNumber)
        {
            LessonDefinition fallback = null;
            int matchingIndex = 0;
            int desiredIndex = Math.Max(0, (roundNumber - 1) / 2);

            foreach (LessonDefinition lesson in lessons)
            {
                if (lesson.ActivityId != activityId)
                {
                    continue;
                }

                if (fallback == null)
                {
                    fallback = lesson;
                }
                if (matchingIndex == desiredIndex)
                {
                    return lesson;
                }

                matchingIndex++;
            }

            return fallback;
        }

        public static bool IsLessonUnlocked(string lessonId, Func<string, bool> isLessonMastered)
        {
            LessonDefinition lesson = GetLesson(lessonId);
            if (lesson == null)
            {
                return false;
            }

            if (lesson.PrerequisiteLessonIds == null || lesson.PrerequisiteLessonIds.Length == 0)
            {
                return true;
            }

            if (isLessonMastered == null)
            {
                return false;
            }

            foreach (string prerequisite in lesson.PrerequisiteLessonIds)
            {
                if (!isLessonMastered(prerequisite))
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Difficulty levels for activities.
    /// </summary>
    [Serializable]
    public enum DifficultyLevel
    {
        Easy = 1,
        Medium = 2,
        Hard = 3
    }

    /// <summary>
    /// Types of errors that can occur during an activity.
    /// </summary>
    [Serializable]
    public enum ErrorType
    {
        /// <summary>
        /// Selected wrong quantity.
        /// </summary>
        WrongQuantity,

        /// <summary>
        /// Selected wrong comparison (more/less/equal).
        /// </summary>
        WrongComparison,

        /// <summary>
        /// Jumped in wrong direction.
        /// </summary>
        WrongDirection,

        /// <summary>
        /// Wrong number of jumps.
        /// </summary>
        WrongJumpCount,

        /// <summary>
        /// Ran out of time (if timed activity).
        /// </summary>
        Timeout,

        /// <summary>
        /// Generic/other error.
        /// </summary>
        Other
    }

    [Serializable]
    public enum TechnicalIssueType
    {
        ARPlaneNotFound,
        PlacementLost,
        TrackingLost,
        ObjectInteractionFailed,
        AssetSpawnFailed,
        TimeoutWaitingForAR,
        Other
    }
}
