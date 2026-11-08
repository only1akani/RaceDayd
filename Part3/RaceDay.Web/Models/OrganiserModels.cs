using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models
{//start of namespace

    //one of the organiser's own events, with enrolment counts
    public class OrganiserEvent
    {//start of class

        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string? EventDescription { get; set; }
        public DateTime EventDate { get; set; }
        public string? Location { get; set; }
        public string EventType { get; set; } = "Run";
        public decimal Distance { get; set; }
        public string? BannerUrl { get; set; }
        public int CategoryCount { get; set; }
        public int EnrolmentCount { get; set; }
        public int ConfirmedCount { get; set; }
        public int ResultCount { get; set; }

    }//end of class

    //one participant's enrolment as the organiser sees it, with any result
    public class EventEnrolment
    {//start of class

        public int EnrolmentId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime EnrolmentDate { get; set; }
        public int ParticipantId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string ParticipantEmail { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int? ResultId { get; set; }
        public TimeSpan? FinishTime { get; set; }
        public int? Position { get; set; }

    }//end of class

    //a published result on the public event page
    public class PublicResult
    {//start of class

        public string ParticipantName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public TimeSpan? FinishTime { get; set; }
        public int? Position { get; set; }

    }//end of class

    //the event shown in the tabs at the top of the manage pages
    public class EventNavModel
    {//start of class

        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;

    }//end of class

    //the organiser dashboard
    public class DashboardViewModel
    {//start of class

        public List<OrganiserEvent> Events { get; set; } = new List<OrganiserEvent>();
        public string? ErrorMessage { get; set; }

        public int TotalEvents { get { return Events.Count; } }

        public int TotalEnrolments { get { return Events.Sum(e => e.EnrolmentCount); } }

        public int PendingEnrolments { get { return Events.Sum(e => e.EnrolmentCount - e.ConfirmedCount); } }

        public List<OrganiserEvent> Upcoming
        {
            get { return Events.Where(e => e.EventDate >= DateTime.Now).OrderBy(e => e.EventDate).Take(5).ToList(); }
        }

    }//end of class

    //the create and edit event form
    public class EventFormViewModel
    {//start of class

        public int EventId { get; set; }

        [Required(ErrorMessage = "Please enter the event name.")]
        [StringLength(100, ErrorMessage = "The event name can be at most 100 characters.")]
        [Display(Name = "Event name")]
        public string EventName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "The description can be at most 500 characters.")]
        [Display(Name = "Description")]
        public string? EventDescription { get; set; }

        [Required(ErrorMessage = "Please choose the date and start time.")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date and start time")]
        public DateTime? EventDate { get; set; }

        [StringLength(150, ErrorMessage = "The location can be at most 150 characters.")]
        public string? Location { get; set; }

        [Required(ErrorMessage = "Please choose an event type.")]
        [RegularExpression("^(Run|Walk|Cycle)$", ErrorMessage = "Please choose Run, Walk or Cycle.")]
        [Display(Name = "Event type")]
        public string EventType { get; set; } = "Run";

        [Required(ErrorMessage = "Please enter the distance in km.")]
        [Range(typeof(decimal), "0.1", "1000", ErrorMessage = "The distance must be between 0.1 and 1000 km.")]
        [Display(Name = "Distance (km)")]
        public decimal? Distance { get; set; }

        //the banner already saved for this event (shown when editing)
        public string? BannerUrl { get; set; }

    }//end of class

    //the add and edit category form
    public class CategoryFormViewModel
    {//start of class

        public int EventId { get; set; }
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Please enter the category name.")]
        [StringLength(50, ErrorMessage = "The category name can be at most 50 characters.")]
        [Display(Name = "Category name")]
        public string CategoryName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the distance in km.")]
        [Range(typeof(decimal), "0.1", "1000", ErrorMessage = "The distance must be between 0.1 and 1000 km.")]
        [Display(Name = "Distance (km)")]
        public decimal? Distance { get; set; }

        [Range(typeof(decimal), "0", "100000", ErrorMessage = "The price must be 0 or more.")]
        [Display(Name = "Entry fee (R)")]
        public decimal? Price { get; set; }

        [Range(1, 100000, ErrorMessage = "There must be room for at least 1 participant.")]
        [Display(Name = "Maximum participants")]
        public int? MaxParticipants { get; set; }

    }//end of class

    public class CategoriesPageViewModel
    {//start of class

        public EventDetails Event { get; set; } = new EventDetails();
        public CategoryFormViewModel Form { get; set; } = new CategoryFormViewModel();

    }//end of class

    public class EnrolmentsPageViewModel
    {//start of class

        public EventDetails Event { get; set; } = new EventDetails();
        public List<EventEnrolment> Enrolments { get; set; } = new List<EventEnrolment>();

    }//end of class

    public class ResultsPageViewModel
    {//start of class

        public EventDetails Event { get; set; } = new EventDetails();
        public List<EventEnrolment> Enrolments { get; set; } = new List<EventEnrolment>();

        //results can only be entered once the event has taken place
        public bool Finished { get { return Event.EventDate < DateTime.Now; } }

    }//end of class
}//end of namespace