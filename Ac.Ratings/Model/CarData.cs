namespace Ac.Ratings.Model {
    public class CarData {
        public string? Name { get; set; }
        public string? Brand { get; set; }
        public CarSpecs Specs { get; set; } = new CarSpecs();
        public CarRatings Ratings { get; set; } = new CarRatings();
    }
}
