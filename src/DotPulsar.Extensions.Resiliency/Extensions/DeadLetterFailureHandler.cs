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
	private readonly Func<IMessage, Exception, TimeSpan?>? delayTimeSelector;
	private readonly Func<Exception, IEnumerable<KeyValuePair<string, string?>>> exceptionSerializer;

	public DeadLetterFailureHandler(
		IDeadLetterPolicy deadLetterPolicy,
		Func<Exception, IEnumerable<KeyValuePair<string, string?>>>? exceptionSerializer = null,
		Func<IMessage, Exception, TimeSpan?>? delayTimeSelector = null) {
		this.deadLetterPolicy = deadLetterPolicy ?? throw new ArgumentNullException(nameof(deadLetterPolicy));
		this.delayTimeSelector = delayTimeSelector;
		this.exceptionSerializer = exceptionSerializer ?? SerializeException;

		static IEnumerable<KeyValuePair<string, string?>> SerializeException(Exception exception) {
			yield return new("EXCEPTION_TYPE", exception.GetType().FullName);
			yield return new("EXCEPTION_MESSAGE", exception.Message);
			yield return new("STACK_TRACE", exception.StackTrace);
		}
	}

	public ValueTask HandleAsync(IMessage message, Exception exception, CancellationToken cancellationToken) {
		var properties = exceptionSerializer(exception);
		return deadLetterPolicy.ReconsumeLater(message, delayTime: delayTimeSelector?.Invoke(message, exception), customProperties: properties, cancellationToken: cancellationToken);
	}
}
