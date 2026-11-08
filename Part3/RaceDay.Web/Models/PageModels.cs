namespace RaceDay.Web.Models
{//start of namespace

    //the events page with its filters
    public class HomeViewModel
    {//start of class

        public List<EventSummary> Events { get; set; } = new List<EventSummary>();
        public string? Type { get; set; }
        public string? Search { get; set; }
        public string? ErrorMessage { get; set; }

    }//end of class

    //the event detail page
    public class EventDetailsViewModel
    {//start of class

        public EventDetails Event { get; set; } = new EventDetails();

        //category id and the status of the logged in participant's enrolment in it
        public Dictionary<int, string> MyEnrolments { get; set; } = new Dictionary<int, string>();
        //the published results of the event
        public List<PublicResult> Results { get; set; } = new List<PublicResult>();

    }//end of class
}//end of namespace