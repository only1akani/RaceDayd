namespace RaceDay.Web.Models
{//start of namespace

    //small helpers used by the pages
    public static class UiHelpers
    {//start of class

        //the icon shown for each event type when there is no banner
        public static string EventIcon(string? eventType)
        {
            return eventType switch
            {
                "Cycle" => "bi-bicycle",
                "Walk" => "bi-signpost-split",
                _ => "bi-lightning-charge"
            };
        }

        //47 becomes 47th, 1 becomes 1st
        public static string Ordinal(int number)
        {//start of method

            int lastTwo = number % 100;
            if (lastTwo >= 11 && lastTwo <= 13)
            {
                return $"{number}th";
            }

            return (number % 10) switch
            {
                1 => $"{number}st",
                2 => $"{number}nd",
                3 => $"{number}rd",
                _ => $"{number}th"
            };

        }//end of method

        //shows 00:48:32 as 0:48:32
        public static string FormatTime(TimeSpan? time)
        {
            return time.HasValue ? time.Value.ToString(@"h\:mm\:ss") : "-";
        }

    }//end of class
}//end of namespace