using System.Text.Json.Serialization;
using Ac.Ratings.Core;

namespace Ac.Ratings.Model;

public class CarRatings : ObservableObject {
    private double _cornerHandling;
    private double _brakes;
    private double _realism;
    private double _sound;
    private double _exteriorQuality;
    private double _interiorQuality;
    private double _forceFeedbackQuality;
    private double _funFactor;
    private double _averageRating;

    private bool _turnSignalsDashboard;
    private bool _absOnFlashing;
    private bool _tcOnFlashing;
    private bool _absOff;
    private bool _tcOff;
    private bool _handbrake;
    private bool _lightsDashboard;
    private bool _otherDashboard;
    private bool _turnSignalsExterior;
    private bool _goodQualityLights;
    private bool _emergencyBrakeLights;
    private bool _fogLights;
    private bool _sequentialTurnSignals;
    private bool _animations;
    private bool _extendedPhysics;
    private bool _startupSound;
    private bool _differentDisplays;
    private bool _differentDrivingModes;

    [JsonPropertyName("cornerHandling")] public double CornerHandling {
        get => _cornerHandling;
        set {
            if (SetField(ref _cornerHandling, value)) {
                UpdateAverageRating();
            }
        }
    }

    [JsonPropertyName("brakes")] public double Brakes {
        get => _brakes;
        set {
            if (SetField(ref _brakes, value)) {
                UpdateAverageRating();
            }
        }
    }

    [JsonPropertyName("realism")] public double Realism {
        get => _realism;
        set {
            if (SetField(ref _realism, value)) {
                UpdateAverageRating();
            }
        }
    }

    [JsonPropertyName("sound")] public double Sound {
        get => _sound;
        set {
            if (SetField(ref _sound, value)) {
                UpdateAverageRating();
            }
        }
    }

    [JsonPropertyName("exteriorQuality")] public double ExteriorQuality {
        get => _exteriorQuality;
        set {
            if (SetField(ref _exteriorQuality, value)) {
                UpdateAverageRating();
            }
        }
    }

    [JsonPropertyName("interiorQuality")] public double InteriorQuality {
        get => _interiorQuality;
        set {
            if (SetField(ref _interiorQuality, value)) {
                UpdateAverageRating();
            }
        }
    }

    [JsonPropertyName("forceFeedbackQuality")] public double ForceFeedbackQuality {
        get => _forceFeedbackQuality;
        set {
            if (SetField(ref _forceFeedbackQuality, value)) {
                UpdateAverageRating();
            }
        }
    }

    [JsonPropertyName("funFactor")] public double FunFactor {
        get => _funFactor;
        set {
            if (SetField(ref _funFactor, value)) {
                UpdateAverageRating();
            }
        }
    }

    [JsonPropertyName("averageRating")] public double AverageRating {
        get => _averageRating;
        set => SetField(ref _averageRating, value);
    }

    // Extra Features
    [JsonPropertyName("turnSignalsDashboard")] public bool TurnSignalsDashboard {
        get => _turnSignalsDashboard;
        set => SetField(ref _turnSignalsDashboard, value);
    }

    [JsonPropertyName("absOnFlashing")] public bool AbsOnFlashing {
        get => _absOnFlashing;
        set => SetField(ref _absOnFlashing, value);
    }

    [JsonPropertyName("tcOnFlashing")] public bool TcOnFlashing {
        get => _tcOnFlashing;
        set => SetField(ref _tcOnFlashing, value);
    }

    [JsonPropertyName("absOff")] public bool AbsOff {
        get => _absOff;
        set => SetField(ref _absOff, value);
    }

    [JsonPropertyName("tcOff")] public bool TcOff {
        get => _tcOff;
        set => SetField(ref _tcOff, value);
    }

    [JsonPropertyName("handbrake")] public bool Handbrake {
        get => _handbrake;
        set => SetField(ref _handbrake, value);
    }

    [JsonPropertyName("lightsDashboard")] public bool LightsDashboard {
        get => _lightsDashboard;
        set => SetField(ref _lightsDashboard, value);
    }

    [JsonPropertyName("otherDashboard")] public bool OtherDashboard {
        get => _otherDashboard;
        set => SetField(ref _otherDashboard, value);
    }

    [JsonPropertyName("turnSignalsExterior")] public bool TurnSignalsExterior {
        get => _turnSignalsExterior;
        set => SetField(ref _turnSignalsExterior, value);
    }

    [JsonPropertyName("goodQualityLights")] public bool GoodQualityLights {
        get => _goodQualityLights;
        set => SetField(ref _goodQualityLights, value);
    }

    [JsonPropertyName("emergencyBrakeLights")] public bool EmergencyBrakeLights {
        get => _emergencyBrakeLights;
        set => SetField(ref _emergencyBrakeLights, value);
    }

    [JsonPropertyName("fogLights")] public bool FogLights {
        get => _fogLights;
        set => SetField(ref _fogLights, value);
    }

    [JsonPropertyName("sequentialTurnSignals")] public bool SequentialTurnSignals {
        get => _sequentialTurnSignals;
        set => SetField(ref _sequentialTurnSignals, value);
    }

    [JsonPropertyName("animations")] public bool Animations {
        get => _animations;
        set => SetField(ref _animations, value);
    }

    [JsonPropertyName("extendedPhysics")] public bool ExtendedPhysics {
        get => _extendedPhysics;
        set => SetField(ref _extendedPhysics, value);
    }

    [JsonPropertyName("startupSound")] public bool StartupSound {
        get => _startupSound;
        set => SetField(ref _startupSound, value);
    }

    [JsonPropertyName("differentDisplays")] public bool DifferentDisplays {
        get => _differentDisplays;
        set => SetField(ref _differentDisplays, value);
    }

    [JsonPropertyName("differentDrivingModes")] public bool DifferentDrivingModes {
        get => _differentDrivingModes;
        set => SetField(ref _differentDrivingModes, value);
    }

    private void UpdateAverageRating() {
        var ratings = new List<double> {
            _cornerHandling,
            _brakes,
            _realism,
            _sound,
            _exteriorQuality,
            _interiorQuality,
            _forceFeedbackQuality,
            _funFactor
        };
        AverageRating = ratings.Average();
    }
}