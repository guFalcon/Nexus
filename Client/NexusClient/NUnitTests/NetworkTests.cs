﻿// ***************************************************************************
// This is free and unencumbered software released into the public domain.
// 
// Anyone is free to copy, modify, publish, use, compile, sell, or
// distribute this software, either in source code form or as a compiled
// binary, for any purpose, commercial or non-commercial, and by any
// means.
// 
// In jurisdictions that recognize copyright laws, the author or authors
// of this software dedicate any and all copyright interest in the
// software to the public domain. We make this dedication for the benefit
// of the public at large and to the detriment of our heirs and
// successors. We intend this dedication to be an overt act of
// relinquishment in perpetuity of all present and future rights to this
// software under copyright law.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
// OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// 
// For more information, please refer to <http://unlicense.org>
// ***************************************************************************

using NexusClient.Converters.MessagePack;
using NexusClient.Network.Testing;
using NexusClient.Nexus;
using NexusClient.NUnitTests.Infrastructure;
using NexusClient.Utils;
using NUnit.Framework;

namespace NexusClient.NUnitTests
{
	[TestFixture()]
	public class NetworkTests
	{
		private Nexus<MessagePackConverter, MessagePackSer, MessagePackDes, MessagePackDto> n1;
		private Nexus<MessagePackConverter, MessagePackSer, MessagePackDes, MessagePackDto> n2;

		private TestHandlerGroup hg;

		[SetUp]
		public void Setup()
		{
			Logger.Init();
			var server = new TestServer();
			var transport1 = new TestTransport(server);
			var transport2 = new TestTransport(server);
			var converter = new MessagePackConverter();
			n1 = new Nexus<MessagePackConverter, MessagePackSer, MessagePackDes, MessagePackDto>(transport1,
				converter);
			n1.Initialize();
			n2 = new Nexus<MessagePackConverter, MessagePackSer, MessagePackDes, MessagePackDto>(transport2,
				converter);
			n2.Initialize();

			hg = new TestHandlerGroup(server);
			n1.RegisterOrOverwriteHandlerGroup(hg);
			n1.AddParticipants(n2.UserId);
			n2.RegisterOrOverwriteHandlerGroup(hg);
			n2.AddParticipants(n1.UserId);
		}

		[Test]
		public void AddAndGetHandler()
		{
			var gt = new TestGameTime();
			n1.Message.ToAll().Send(TestType.ELECTION_CALL, new TestContent() {TestField = "test from user1"});
			n2.Message.ToOthers().Send(TestType.ELECTION_CALL, new TestContent() {TestField = "test from user2"});
			n1.Message.ToOthersExcept(n2.UserId).Send(TestType.ELECTION_CALL_ANSWER,
				new TestContent() {TestField = "should not be received"});

			gt.Advance(1);
			n1.Update(gt.Value());
			n2.Update(gt.Value());

			gt.Advance(1);
			n1.Update(gt.Value());
			n2.Update(gt.Value());

			gt.Advance(1);
			n1.Update(gt.Value());
			n2.Update(gt.Value());
		}
	}
}