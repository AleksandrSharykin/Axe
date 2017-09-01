using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.WebSockets;
using Axe.Models;
using Axe.Dto;

namespace Axe.Managers
{
    /// <summary>
    /// Interface declares operations governed by QuizController
    /// </summary>
    public interface IQuizManager
    {
        /// <summary>
        /// Returns a list of available quizes
        /// </summary>
        Task<Response<List<RealtimeQuiz>>> Index();

        /// <summary>
        /// Returns quiz instance for edit
        /// </summary>
        Task<Response<RealtimeQuiz>> InputGet(Request<int?> request);

        /// <summary>
        /// Validates and saves quiz edits
        /// </summary>
        Task<Response<RealtimeQuiz>> InputPost(Request<RealtimeQuiz> request);

        /// <summary>
        /// Returns information about quiz participants
        /// </summary>
        Task<Response<QuizDetails>> Details(Request<int> request);

        /// <summary>
        /// Gets quiz data for asking questions
        /// </summary>
        Task<Response<RealtimeQuiz>> Ask(Request<int> request);

        /// <summary>
        /// Gets quiz data for answering
        /// </summary>
        Task<Response<RealtimeQuiz>> Answer(Request<int> request);

        /// <summary>
        /// Accepts WebSocket communication request
        /// </summary>
        Task Connect(Request<WebSocket> request);

        /// <summary>
        /// Marks an answer from participant
        /// </summary>
        Task<Response<QuizMessage>> Mark(Request<QuizMessage> request);
    }
}
