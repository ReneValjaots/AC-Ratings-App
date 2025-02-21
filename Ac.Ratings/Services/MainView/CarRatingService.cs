using Ac.Ratings.Model;

namespace Ac.Ratings.Services.MainView {
    public class CarRatingService {
        public static void ResetExtraFeatureValues(Car selectedCar) {
            selectedCar.Ratings.TurnSignalsDashboard = false;
            selectedCar.Ratings.AbsOnFlashing = false;
            selectedCar.Ratings.TcOnFlashing = false;
            selectedCar.Ratings.AbsOff = false;
            selectedCar.Ratings.TcOff = false;
            selectedCar.Ratings.Handbrake = false;
            selectedCar.Ratings.LightsDashboard = false;
            selectedCar.Ratings.OtherDashboard = false;
            selectedCar.Ratings.TurnSignalsExterior = false;
            selectedCar.Ratings.GoodQualityLights = false;
            selectedCar.Ratings.EmergencyBrakeLights = false;
            selectedCar.Ratings.FogLights = false;
            selectedCar.Ratings.SequentialTurnSignals = false;
            selectedCar.Ratings.Animations = false;
            selectedCar.Ratings.ExtendedPhysics = false;
            selectedCar.Ratings.StartupSound = false;
            selectedCar.Ratings.DifferentDisplays = false;
            selectedCar.Ratings.DifferentDrivingModes = false;
        }

        public static void ResetRatingValues(Car selectedCar) {
            selectedCar.Ratings.CornerHandling = 0;
            selectedCar.Ratings.Brakes = 0;
            selectedCar.Ratings.Realism = 0;
            selectedCar.Ratings.Sound = 0;
            selectedCar.Ratings.ExteriorQuality = 0;
            selectedCar.Ratings.InteriorQuality = 0;
            selectedCar.Ratings.ForceFeedbackQuality = 0;
            selectedCar.Ratings.FunFactor = 0;
            selectedCar.Ratings.AverageRating = 0;
        }
    }
}
