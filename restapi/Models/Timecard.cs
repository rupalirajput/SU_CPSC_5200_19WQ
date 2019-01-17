using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;

namespace restapi.Models
{
    public class Timecard
    {
        public Timecard(int resource)
        {
            Resource = resource;
            UniqueIdentifier = Guid.NewGuid();
            Identity = new TimecardIdentity();
            Lines = new List<AnnotatedTimecardLine>();
            Transitions = new List<Transition> {
                new Transition(new Entered() { Resource = resource })
            };
        }

        public int Resource { get; private set; }

        [JsonProperty("id")]
        public TimecardIdentity Identity { get; private set; }

        public TimecardStatus Status
        {
            get
            {
                return Transitions
                    .OrderByDescending(t => t.OccurredAt)
                    .First()
                    .TransitionedTo;
            }
        }

        public DateTime Opened;

        [JsonProperty("recId")]
        public int RecordIdentity { get; set; } = 0;

        [JsonProperty("recVersion")]
        public int RecordVersion { get; set; } = 0;

        public Guid UniqueIdentifier { get; set; }

        [JsonIgnore]
        public IList<AnnotatedTimecardLine> Lines { get; set; }

        [JsonIgnore]
        public IList<Transition> Transitions { get; set; }

        public IList<ActionLink> Actions { get => GetActionLinks(); }

        [JsonProperty("documentation")]
        public IList<DocumentLink> Documents { get => GetDocumentLinks(); }

        public string Version { get; set; } = "timecard-0.1";

        private IList<ActionLink> GetActionLinks()
        {
            var links = new List<ActionLink>();

            switch (Status)
            {
                case TimecardStatus.Draft:
                    links.Add(new ActionLink()
                    {
                        Method = Method.Post,
                        Type = ContentTypes.Cancellation,
                        Relationship = ActionRelationship.Cancel,
                        Reference = $"/timesheets/{Identity.Value}/cancellation"
                    });

                    links.Add(new ActionLink()
                    {
                        Method = Method.Post,
                        Type = ContentTypes.Submittal,
                        Relationship = ActionRelationship.Submit,
                        Reference = $"/timesheets/{Identity.Value}/submittal"
                    });

                    links.Add(new ActionLink()
                    {
                        Method = Method.Post,
                        Type = ContentTypes.TimesheetLine,
                        Relationship = ActionRelationship.RecordLine,
                        Reference = $"/timesheets/{Identity.Value}/lines"
                    });

                    break;

                case TimecardStatus.Submitted:
                    links.Add(new ActionLink()
                    {
                        Method = Method.Post,
                        Type = ContentTypes.Cancellation,
                        Relationship = ActionRelationship.Cancel,
                        Reference = $"/timesheets/{Identity.Value}/cancellation"
                    });

                    links.Add(new ActionLink()
                    {
                        Method = Method.Post,
                        Type = ContentTypes.Rejection,
                        Relationship = ActionRelationship.Reject,
                        Reference = $"/timesheets/{Identity.Value}/rejection"
                    });

                    links.Add(new ActionLink()
                    {
                        Method = Method.Post,
                        Type = ContentTypes.Approval,
                        Relationship = ActionRelationship.Approve,
                        Reference = $"/timesheets/{Identity.Value}/approval"
                    });

                    break;

                case TimecardStatus.Approved:
                    // terminal state, nothing possible here
                    break;

                case TimecardStatus.Cancelled:
                    // terminal state, nothing possible here
                    break;
            }

            return links;
        }

        private IList<DocumentLink> GetDocumentLinks()
        {
            var links = new List<DocumentLink>();

            links.Add(new DocumentLink()
            {
                Method = Method.Get,
                Type = ContentTypes.Transitions,
                Relationship = DocumentRelationship.Transitions,
                Reference = $"/timesheets/{Identity.Value}/transitions"
            });

            if (this.Lines.Count > 0)
            {
                links.Add(new DocumentLink()
                {
                    Method = Method.Get,
                    Type = ContentTypes.TimesheetLine,
                    Relationship = DocumentRelationship.Lines,
                    Reference = $"/timesheets/{Identity.Value}/lines"
                });
            }

            if (this.Status == TimecardStatus.Submitted)
            {
                links.Add(new DocumentLink()
                {
                    Method = Method.Get,
                    Type = ContentTypes.Transitions,
                    Relationship = DocumentRelationship.Submittal,
                    Reference = $"/timesheets/{Identity.Value}/submittal"
                });
            }

            return links;
        }

        public AnnotatedTimecardLine AddLine(TimecardLine timecardLine)
        {
            var annotatedLine = new AnnotatedTimecardLine(timecardLine);

            Lines.Add(annotatedLine);

            return annotatedLine;
        }

        public AnnotatedTimecardLine ReplaceLine(AnnotatedTimecardLine old_line, TimecardLine timecardLine, Guid uniqueIdentifier)
        {
            var annotatedLine = new AnnotatedTimecardLine(timecardLine);

            var index = Lines.IndexOf(Lines.Where(x => x.UniqueIdentifier == uniqueIdentifier).FirstOrDefault());

            Lines.RemoveAt(index);

            old_line.Year = annotatedLine.Year;

            old_line.Project = annotatedLine.Project;

            old_line.Week = annotatedLine.Week;

            old_line.Hours = annotatedLine.Hours;

            old_line.Day = annotatedLine.Day;

            old_line.UniqueIdentifier = uniqueIdentifier;

            Lines.Insert(index, old_line);

            return old_line;
        }

        public AnnotatedTimecardLine UpdateLine(AnnotatedTimecardLine old_line, TimecardLine timecardLine, Guid uniqueIdentifier)
        {
            // columnname that needs to be changed
            string column = timecardLine.Path;

            // value of that column that needs to be changed/updated/removed/replaced/added
            string value = timecardLine.Value;

            // supports only `replace` operation
            if (timecardLine.Op == "replace")
            {
                switch (column)
                {
                    case "week":
                        old_line.Week = int.Parse(value);
                        old_line.Year = old_line.Year;
                        old_line.Day = old_line.Day;
                        old_line.Project = old_line.Project;
                        old_line.Hours = old_line.Hours;
                        break;

                    case "year":
                        old_line.Year = int.Parse(value);
                        old_line.Week = old_line.Week;
                        old_line.Day = old_line.Day;
                        old_line.Project = old_line.Project;
                        old_line.Hours = old_line.Hours;
                        break;

                    case "day":
                        old_line.Day = ((DayOfWeek)Enum.Parse(typeof(DayOfWeek), CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower())));
                        old_line.Week = old_line.Week;
                        old_line.Year = old_line.Year;
                        old_line.Project = old_line.Project;
                        old_line.Hours = old_line.Hours;
                        break;

                    case "project":
                        old_line.Project = value;
                        old_line.Week = old_line.Week;
                        old_line.Year = old_line.Year;
                        old_line.Day = old_line.Day;
                        old_line.Hours = old_line.Hours;
                        break;

                    case "hours":
                        old_line.Hours = float.Parse(value);
                        old_line.Week = old_line.Week;
                        old_line.Year = old_line.Year;
                        old_line.Day = old_line.Day;
                        old_line.Project = old_line.Project;
                        break;

                    default:
                        break;
                }
            }

            var index = Lines.IndexOf(Lines.Where(x => x.UniqueIdentifier == uniqueIdentifier).FirstOrDefault());

            Lines.RemoveAt(index);

            old_line.UniqueIdentifier = uniqueIdentifier;

            Lines.Insert(index, old_line);

            return old_line;
        }
    }
}