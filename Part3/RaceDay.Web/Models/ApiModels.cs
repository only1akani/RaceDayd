namespace RaceDay.Web.Models
{//start of namespace

    //one event in a list
    public class EventSummary
    {//start of class

        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string? EventDescription { get; set; }
        public DateTime EventDate { get; set; }
        public string? Location { get; set; }
        public string EventType { get; set; } = "Run";
        public decimal Distance { get; set; }
        public string? BannerUrl { get; set; }
        public int OrganiserId { get; set; }

    }//end of class

    //a distance or age category inside an event
    public class CategoryInfo
    {//start of class

        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public decimal Price { get; set; }
        public int MaxParticipants { get; set; }

    }//end of class

    //the full details of one event
    public class EventDetails
    {//start of class

        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string? EventDescription { get; set; }
        public DateTime EventDate { get; set; }
        public string? Location { get; set; }
        public string EventType { get; set; } = "Run";
        public decimal Distance { get; set; }
        public string? BannerUrl { get; set; }
        public int OrganiserId { get; set; }
        public string OrganiserName { get; set; } = string.Empty;
        public List<CategoryInfo> Categories { get; set; } = new List<CategoryInfo>();

    }//end of class

    //one enrolment of the logged in participant
    public class MyEnrolment
    {//start of class

        public int EnrolmentId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime EnrolmentDate { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string? Location { get; set; }
        public string EventType { get; set; } = "Run";
        public string? BannerUrl { get; set; }

    }//end of class

    //one result of the logged in participant (used in Step 5B)
    public class MyResult
    {//start of class

        public int ResultId { get; set; }

        public int EnrolmentId { get; set; }
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string? Location { get; set; }
        public string EventType { get; set; } = "Run";
        public string CategoryName { get; set; } = string.Empty;
        public TimeSpan? FinishTime { get; set; }
        public int? Position { get; set; }
        public int TotalFinishers { get; set; }

    }//end of class

    //the profile of the logged in user (used in Step 5B)
    public class UserProfile
    {//start of class

        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }

    }//end of class
}//end of namespace