using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe.Managers
{
    /// <summary>
    /// Class contains messages for notification about validation errors
    /// </summary>
    public class ValidationMessages
    {
        private static ValidationMessages instance;

        /// <summary>
        /// Gets singletone instance of messages container
        /// </summary>
        public static ValidationMessages Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ValidationMessages();
                }
                return instance;
            }
        }

        private ValidationMessages() { }

        public string UnknownTechnology => "Unknown technology";

        public string AssessmentCannotAppointExaminer => "Cannot appoint other users as examiners";

        public string AssessmentExpertAssign(string technologyName) => $"Only {technologyName} expert can assign skill assessment";

        public string AssessmentSelf => "Cannot assign skill assessment to self";

        public string AssessmentPastEvent => "Event has already happened";

        public string AssessmentInvalidDetails => "Invalid assessment details";

        public string AssessmentMarked => "Assessment has already been marked";

        public string AssessmentNonExaminerMark => "Only examiner can mark assessment";

        public string AssessmentCannotDelete => "You cannot delete this record";


        public string TaskNoQuestions => "Test task should contain at least one question";

        public string TaskExpertInput(string technologyName) => $"Only expert can create {technologyName} tasks";

        public string TaskExpertDelete(string technologyName) => $"Only expert can delete {technologyName} tasks";


        public string QuestionExpertInput(string technologyName) => $"Only {technologyName} experts can write questions";

        public string QuestionTwoChoiceOptions => "Question should have at least two options";

        public string QuestionNeedAnswer => "Question should have an answer";

        public string QuestionExpertDelete(string technologyName) => $"Only expert can delete {technologyName} questions";


        public string TechnologyExpertInput => "Only expert can edit technology";

        public string TechnologyDuplicate(string technologyName) => $"{technologyName} technology is already exists";

        public string TechnologyExpertDelete => "Only expert can delete technology";
    }
}
