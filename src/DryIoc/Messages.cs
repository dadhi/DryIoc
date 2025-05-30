// <auto-generated/>
/*
The MIT License (MIT)

Copyright (c) 2016-2024 Maksim Volkau

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included 
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DryIoc.Messages;

#nullable disable

/// <summary>Base type for messages</summary>
public interface IMessage<out TResponse> { }

/// <summary>Type for an empty response</summary>
public struct EmptyResponse
{
    /// <summary>Single value of empty response</summary>
    public static readonly EmptyResponse Value = new EmptyResponse();

    /// <summary>Single completed task for the empty response</summary>
    public static readonly Task<EmptyResponse> Task = System.Threading.Tasks.Task.FromResult(Value);
}

/// <summary>Message extensions</summary>
public static class MessageExtensions
{
    /// <summary>Converts the task to empty response task</summary>
    public static async Task<EmptyResponse> ToEmptyResponse(this Task task)
    {
        await task;
        return EmptyResponse.Value;
    }
}

/// <summary>Message with empty response</summary>
public interface IMessage : IMessage<EmptyResponse> { }

/// <summary>Base message handler</summary>
public interface IMessageHandler<in M, R> where M : IMessage<R>
{
    /// <summary>Generic handler</summary>
    Task<R> Handle(M message, CancellationToken cancellationToken);
}

/// <summary>Base message handler for message with empty response</summary>
public interface IMessageHandler<in M> : IMessageHandler<M, EmptyResponse> where M : IMessage<EmptyResponse> { }

/// <summary>Message handler middleware for message with empty response</summary>
public interface IMessageMiddleware<in M> : IMessageMiddleware<M, EmptyResponse> { }

/// <summary>Message handler middleware to handle the message and pass the result to the next middleware</summary>
public interface IMessageMiddleware<in M, R>
{
    /// <summary>`0` means the default registration order,
    /// lesser numbers including the `-1`, `-2` mean execute as a first,
    /// bigger numbers mean execute as a last</summary>
    int RelativeOrder { get; }

    /// <summary>Handles message and passes to the next middleware</summary>
    Task<R> Handle(M message, CancellationToken cancellationToken, Func<Task<R>> nextMiddleware);
}

/// <summary>Base class for implementing async handlers</summary>
public abstract class AsyncMessageHandler<M, R> : IMessageHandler<M, R>
    where M : IMessage<R>
{
    /// <summary>Base method to implement in your inheritor</summary>
    protected abstract Task<R> Handle(M message, CancellationToken cancellationToken);

    async Task<R> IMessageHandler<M, R>.Handle(M message, CancellationToken cancellationToken) =>
        await Handle(message, cancellationToken).ConfigureAwait(false);
}

/// <summary>Sequential middleware type of message handler decorator</summary>
public class MiddlewareMessageHandler<M, R> : IMessageHandler<M, R> where M : IMessage<R>
{
    private readonly IMessageHandler<M, R> _handler;
    private readonly IEnumerable<IMessageMiddleware<M, R>> _middlewares;

    /// <summary>Decorates message handler with optional middlewares</summary>
    public MiddlewareMessageHandler(IMessageHandler<M, R> handler, IEnumerable<IMessageMiddleware<M, R>> middlewares)
    {
        _handler = handler;
        _middlewares = middlewares;
    }

    /// <summary>Composes middlewares with handler</summary>
    public Task<R> Handle(M message, CancellationToken cancellationToken)
    {
        return _middlewares
            .OrderBy(x => x.RelativeOrder)
            .Reverse()
            .Aggregate(
                new Func<Task<R>>(() => _handler.Handle(message, cancellationToken)),
                (f, middleware) => () => middleware.Handle(message, cancellationToken, f))
            .Invoke();
    }
}

/// <summary>Broadcasting type of message handler decorator</summary>
public class BroadcastMessageHandler<M> : IMessageHandler<M, EmptyResponse>
    where M : IMessage<EmptyResponse>
{
    private readonly IEnumerable<IMessageHandler<M, EmptyResponse>> _handlers;

    /// <summary>Constructs the hub with the handler and optional middlewares</summary>
    public BroadcastMessageHandler(IEnumerable<IMessageHandler<M, EmptyResponse>> handlers) =>
        _handlers = handlers;

    /// <summary>Composes middlewares with handler</summary>
    public async Task<EmptyResponse> Handle(M message, CancellationToken cancellationToken)
    {
        await Task.WhenAll(_handlers.Select(h => h.Handle(message, cancellationToken)));
        return EmptyResponse.Value;
    }
}

// todo: @feature add SendToAll with the reducing the results
// todo: @feature add NotifyAll without waiting for results
/// <summary>The central mediator entry-point</summary>
public class MessageMediator
{
    private readonly IResolver _resolver;

    /// <summary>Constructs the mediator</summary>
    public MessageMediator(IResolver resolver) =>
        _resolver = resolver;

    /// <summary>Sends the message with response to the resolved Single handler</summary>
    public Task<R> Send<M, R>(M message, CancellationToken cancellationToken) where M : IMessage<R> =>
        _resolver.Resolve<IMessageHandler<M, R>>().Handle(message, cancellationToken);

    /// <summary>Sends the message with empty response to resolved Single handler</summary>
    public Task Send<M>(M message, CancellationToken cancellationToken) where M : IMessage<EmptyResponse> =>
        _resolver.Resolve<IMessageHandler<M, EmptyResponse>>().Handle(message, cancellationToken);
}

#nullable restore