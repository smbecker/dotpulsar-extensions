// /*
//  * Licensed under the Apache License, Version 2.0 (the "License")
//  * you may not use this file except in compliance with the License.
//  * You may obtain a copy of the License at
//  *
//  *   http://www.apache.org/licenses/LICENSE-2.0
//  *
//  * Unless required by applicable law or agreed to in writing, software
//  * distributed under the License is distributed on an "AS IS" BASIS,
//  * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  * See the License for the specific language governing permissions and
//  * limitations under the License.
//  */

using DotPulsar.Abstractions;

namespace DotPulsar.Extensions;

public class DeadLetterFailureHandler : IConsumerFailureHandler
{
	private readonly IDeadLetterPolicy deadLetterPolicy;
	private readonly Func<IMessage, Exception, bool>? retryExceptionHandler;
	private readonly Func<IMessage, Exception, TimeSpan?>? delayTimeSelector;
	private readonly Func<Exception, IEnumerable<KeyValuePair<string, string?>>> exceptionSerializer;
	private readonly Func<IMessage, Exception, IEnumerable<KeyValuePair<string, string?>>>? customPropertyProvider;

	public DeadLetterFailureHandler(
		IDeadLetterPolicy deadLetterPolicy,
		Func<IMessage, Exception, bool>? retryExceptionHandler = null,
		Func<Exception, IEnumerable<KeyValuePair<string, string?>>>? exceptionSerializer = null,
		Func<IMessage, Exception, IEnumerable<KeyValuePair<string, string?>>>? customPropertyProvider = null,
		Func<IMessage, Exception, TimeSpan?>? delayTimeSelector = null) {
		this.deadLetterPolicy = deadLetterPolicy ?? throw new ArgumentNullException(nameof(deadLetterPolicy));
		this.retryExceptionHandler = retryExceptionHandler;
		this.delayTimeSelector = delayTimeSelector;
		this.exceptionSerializer = exceptionSerializer ?? DeadLetterPolicy.SerializeException;
		this.customPropertyProvider = customPropertyProvider;
	}

	public ValueTask HandleAsync(IMessage message, Exception exception, CancellationToken cancellationToken) {
		return deadLetterPolicy.ReconsumeLater(
			message,
			delayTime: delayTimeSelector?.Invoke(message, exception),
			customRetryProperties: customPropertyProvider?.Invoke(message, exception),
			customDlqProperties: exceptionSerializer(exception),
			preventRetry: retryExceptionHandler == null || !retryExceptionHandler(message, exception),
			cancellationToken: cancellationToken);
	}
}
