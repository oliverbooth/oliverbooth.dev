using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using OliverBooth.Data.Web;
using ISession = OliverBooth.Data.Web.ISession;

namespace OliverBooth.Services;

public interface ISessionService
{
    /// <summary>
    ///     Creates a new session for the specified user.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <param name="user">The user.</param>
    /// <returns>The newly-created session.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <para><paramref name="request" /> is <see langword="null" />.</para>
    ///     -or-
    ///     <para><paramref name="user" /> is <see langword="null" />.</para>
    /// </exception>
    ISession CreateSession(HttpRequest request, IUser user);

    /// <summary>
    ///     Deletes the specified session.
    /// </summary>
    /// <param name="session">The session to delete.</param>
    /// <exception cref="ArgumentNullException"><paramref name="session" /> is <see langword="null" />.</exception>
    void DeleteSession(ISession session);

    /// <summary>
    ///     Saves a session cookie to the client's browser.
    /// </summary>
    /// <param name="response">The response to edit.</param>
    /// <param name="session">The session to write.</param>
    /// <exception cref="ArgumentNullException">
    ///     <para><paramref name="request" /> is <see langword="null" />.</para>
    ///     -or-
    ///     <para><paramref name="session" /> is <see langword="null" />.</para>
    /// </exception>
    void SaveSessionCookie(HttpResponse response, ISession session);
    
    /// <summary>
    ///     Attempts to find a session with the specified ID.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="session">
    ///     When this method returns, contains the session with the specified ID, if the session is found; otherwise,
    ///     <see langword="null" />.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if a session with the specified ID is found; otherwise, <see langword="false" />.
    /// </returns>
    bool TryGetSession(Guid sessionId, [NotNullWhen(true)] out ISession? session);

    /// <summary>
    ///     Attempts to find the session associated with the HTTP request.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <param name="session">
    ///     When this method returns, contains the session with the specified request, if the user is found; otherwise,
    ///     <see langword="null" />.
    /// </param>
    /// <returns><see langword="true" /> if the session was found; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request" /> is <see langword="null" />.</exception>
    bool TryGetSession(HttpRequest request, [NotNullWhen(true)] out ISession? session);

    /// <summary>
    ///     Validates the session with the incoming HTTP request.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <param name="session">The session.</param>
    /// <returns><see langword="true" /> if the session is valid; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <para><paramref name="request" /> is <see langword="null" />.</para>
    ///     -or-
    ///     <para><paramref name="session" /> is <see langword="null" />.</para>
    /// </exception>
    bool ValidateSession(HttpRequest request, ISession session);
}
