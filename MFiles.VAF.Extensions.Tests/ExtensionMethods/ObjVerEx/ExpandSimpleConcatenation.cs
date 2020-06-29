﻿using MFiles.VAF.Configuration;
using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Tests.ExtensionMethods.ObjVerEx
{
	[TestClass]
	public class ExpandSimpleConcatenation
		: TestBaseWithVaultMock
	{
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void NullObjVerExThrows()
		{
			((Common.ObjVerEx)null).ExpandSimpleConcatenation("hello world");
		}

		[TestMethod]
		public void NullStringReturnsEmptyString()
		{
			Assert.AreEqual(string.Empty, new Common.ObjVerEx().ExpandSimpleConcatenation(null));
		}

		[TestMethod]
		public void BlankStringReturnsEmptyString()
		{
			Assert.AreEqual(string.Empty, new Common.ObjVerEx().ExpandSimpleConcatenation(""));
		}

		[TestMethod]
		public void SingleInternalID()
		{
			var vaultMock = this.GetVaultMock();
			var objectVersionAndPropertiesMock = this.GetObjectVersionAndPropertiesMock(vaultMock);
			var objVerEx = new Common.ObjVerEx(vaultMock.Object, objectVersionAndPropertiesMock.Object);
			Assert.AreEqual("hello 123 world", objVerEx.ExpandSimpleConcatenation("hello %INTERNALID% world"));
		}

		[TestMethod]
		public void MultipleInternalID()
		{
			var vaultMock = this.GetVaultMock();
			var objectVersionAndPropertiesMock = this.GetObjectVersionAndPropertiesMock(vaultMock);
			var objVerEx = new Common.ObjVerEx(vaultMock.Object, objectVersionAndPropertiesMock.Object);
			Assert.AreEqual("hello 123 world 123", objVerEx.ExpandSimpleConcatenation("hello %INTERNALID% world %INTERNALID%"));
		}

		[TestMethod]
		public void SingleExternalID()
		{
			var vaultMock = this.GetVaultMock();
			var objectVersionAndPropertiesMock = this.GetObjectVersionAndPropertiesMock(vaultMock);
			var objVerEx = new Common.ObjVerEx(vaultMock.Object, objectVersionAndPropertiesMock.Object);
			Assert.AreEqual("hello 123ABCDEF123 world", objVerEx.ExpandSimpleConcatenation("hello %EXTERNALID% world"));
		}

		[TestMethod]
		public void MultipleExternalID()
		{
			var vaultMock = this.GetVaultMock();
			var objectVersionAndPropertiesMock = this.GetObjectVersionAndPropertiesMock(vaultMock);
			var objVerEx = new Common.ObjVerEx(vaultMock.Object, objectVersionAndPropertiesMock.Object);
			Assert.AreEqual("hello 123ABCDEF123 world 123ABCDEF123", objVerEx.ExpandSimpleConcatenation("hello %EXTERNALID% world %EXTERNALID%"));
		}

		[TestMethod]
		public void PropertyIDWithNumber()
		{
			var vaultMock = this.GetVaultMock();
			var objectVersionAndPropertiesMock = this.GetObjectVersionAndPropertiesMock
			(
				vaultMock,
				propertyValues: new Tuple<int, MFDataType, object>(0, MFDataType.MFDatatypeText, "hello world")
			);
			var objVerEx = new Common.ObjVerEx(vaultMock.Object, objectVersionAndPropertiesMock.Object);
			Assert.AreEqual("document is called: hello world", objVerEx.ExpandSimpleConcatenation("document is called: %PROPERTY_0%"));
		}

		[TestMethod]
		public void PropertyIDWithAlias()
		{
			var vaultMock = this.GetVaultMock
			(
				new Tuple<string, int>("MF.PD.Title", 0)
			);
			var objectVersionAndPropertiesMock = this.GetObjectVersionAndPropertiesMock
			(
				vaultMock,
				propertyValues: new Tuple<int, MFDataType, object>(0, MFDataType.MFDatatypeText, "hello world")
			);
			var objVerEx = new Common.ObjVerEx(vaultMock.Object, objectVersionAndPropertiesMock.Object);
			Assert.AreEqual("document is called: hello world", objVerEx.ExpandSimpleConcatenation("document is called: %PROPERTY_{MF.PD.Title}%"));
		}

		[TestMethod]
		public void InternalIDPlaceholderIsExtracted()
		{
			var input = "document ID: %INTERNALID%";
			var matches = ObjVerExExtensionMethods
				.ExtractPlaceholders
				.Matches(input);
			Assert.AreEqual(1, matches.Count);
			var match = matches[0];
			Assert.IsTrue(match.Success);
			Assert.AreEqual("INTERNALID", match.Groups["type"]?.Value);
		}

		[TestMethod]
		public void ExternalIDPlaceholderIsExtracted()
		{
			var input = "document external ID: %EXTERNALID%";
			var matches = ObjVerExExtensionMethods
				.ExtractPlaceholders
				.Matches(input);
			Assert.AreEqual(1, matches.Count);
			var match = matches[0];
			Assert.IsTrue(match.Success);
			Assert.AreEqual("EXTERNALID", match.Groups["type"]?.Value);
		}

		[TestMethod]
		public void IntegerPropertyReferencePlaceholderIsExtracted()
		{
			var vaultMock = this.GetVaultMock();
			var input = "document name: %PROPERTY_0%";
			var matches = ObjVerExExtensionMethods
				.ExtractPlaceholders
				.Matches(input);
			Assert.AreEqual(1, matches.Count);
			var match = matches[0];
			Assert.IsTrue(match.Success);
			Assert.AreEqual("PROPERTY", match.Groups["type"]?.Value);
			var propertyIds = match.Groups["aliasorid"].ToPropertyIds(vaultMock.Object);
			Assert.AreEqual(1, propertyIds.Count); // Only got one property.
			Assert.AreEqual(0, propertyIds[0]); // Property ID was 0.
		}

		[TestMethod]
		public void AliasPropertyReferencePlaceholderIsExtracted()
		{
			var vaultMock = this.GetVaultMock
			(
				new Tuple<string, int>("MF.PD.Title", 1234)
			);
			var input = "document name: %PROPERTY_{MF.PD.Title}%";
			var matches = ObjVerExExtensionMethods
				.ExtractPlaceholders
				.Matches(input);
			Assert.AreEqual(1, matches.Count);
			var match = matches[0];
			Assert.IsTrue(match.Success);
			Assert.AreEqual("PROPERTY", match.Groups["type"]?.Value);
			var propertyIds = match.Groups["aliasorid"].ToPropertyIds(vaultMock.Object);
			Assert.AreEqual(1, propertyIds.Count); // Only got one property.
			Assert.AreEqual(1234, propertyIds[0]); // Property ID was 1234.
		}

		[TestMethod]
		public void IndirectPropertyReferencePlaceholderIsExtractedIDs()
		{
			var vaultMock = this.GetVaultMock();
			var input = "document name: %PROPERTY_123.PROPERTY_456.PROPERTY_789%";
			var matches = ObjVerExExtensionMethods
				.ExtractPlaceholders
				.Matches(input);

			// One match.
			Assert.AreEqual(1, matches.Count);
			var match = matches[0];
			Assert.IsTrue(match.Success);

			// Still a property
			Assert.AreEqual("PROPERTY", match.Groups["type"]?.Value);

			// Get hte property IDs.
			var propertyIds = match.Groups["aliasorid"].ToPropertyIds(vaultMock.Object);
			Assert.AreEqual(3, propertyIds.Count);
			Assert.AreEqual(123, propertyIds[0]); // First is 123
			Assert.AreEqual(456, propertyIds[1]); // Then 456
			Assert.AreEqual(789, propertyIds[2]); // Finally 789
		}

		[TestMethod]
		public void IndirectPropertyReferencePlaceholderIsExtractedAliases()
		{
			var vaultMock = this.GetVaultMock
			(
				new Tuple<string, int>("MF.PD.Contact", 987),
				new Tuple<string, int>("MF.PD.Customer", 654),
				new Tuple<string, int>("MF.PD.Country", 321)
			);
			var input = "document name: %PROPERTY_{MF.PD.Contact}.PROPERTY_{MF.PD.Customer}.PROPERTY_{MF.PD.Country}%";
			var matches = ObjVerExExtensionMethods
				.ExtractPlaceholders
				.Matches(input);

			// One match.
			Assert.AreEqual(1, matches.Count);
			var match = matches[0];
			Assert.IsTrue(match.Success);

			// Still a property
			Assert.AreEqual("PROPERTY", match.Groups["type"]?.Value);

			// Get hte property IDs.
			var propertyIds = match.Groups["aliasorid"].ToPropertyIds(vaultMock.Object);
			Assert.AreEqual(3, propertyIds.Count);
			Assert.AreEqual(987, propertyIds[0]); // First is 987
			Assert.AreEqual(654, propertyIds[1]); // Then 654
			Assert.AreEqual(321, propertyIds[2]); // Finally 321
		}

		[TestMethod]
		public void IndirectPropertyReferencePlaceholderIsExtractedMixture()
		{
			var vaultMock = this.GetVaultMock
			(
				new Tuple<string, int>("MF.PD.Contact", 987),
				new Tuple<string, int>("MF.PD.Country", 321)
			);
			var input = "document name: %PROPERTY_{MF.PD.Contact}.PROPERTY_123.PROPERTY_{MF.PD.Country}%";
			var matches = ObjVerExExtensionMethods
				.ExtractPlaceholders
				.Matches(input);

			// One match.
			Assert.AreEqual(1, matches.Count);
			var match = matches[0];
			Assert.IsTrue(match.Success);

			// Still a property
			Assert.AreEqual("PROPERTY", match.Groups["type"]?.Value);

			// Get hte property IDs.
			var propertyIds = match.Groups["aliasorid"].ToPropertyIds(vaultMock.Object);
			Assert.AreEqual(3, propertyIds.Count);
			Assert.AreEqual(987, propertyIds[0]); // First is 987
			Assert.AreEqual(123, propertyIds[1]); // Then 123
			Assert.AreEqual(321, propertyIds[2]); // Finally 321
		}
		
		[TestMethod]
		public void IndirectPropertyReferencePlaceholderIDs()
		{
			// Create our mock objects.
			var vaultMock = this.GetVaultMock();
			var objectTwo = new Common.ObjVerEx
			(
				vaultMock.Object,
				this.GetObjectVersionAndPropertiesMock(vaultMock, 0, 456).Object
			);
			var objectOne = new Common.ObjVerEx
			(
				vaultMock.Object,
				this.GetObjectVersionAndPropertiesMock(vaultMock, 0, 123).Object
			);
			var root = new Common.ObjVerEx
			(
				vaultMock.Object,
				this.GetObjectVersionAndPropertiesMock(vaultMock, 0, 1).Object
			);

			// Create our methods for passing data around.
			Func<Common.ObjVerEx, int, string> getPropertyText = (Common.ObjVerEx o, int id) =>
			{
				switch( id )
				{
					case 789:
						return "hello world";
					default:
						Assert.Fail("Unexpected property retrieval");
						throw new InvalidOperationException();
				}
			};
			Func<Common.ObjVerEx, int, Common.ObjVerEx> getDirectReference = (Common.ObjVerEx o, int id) =>
			{
				switch( id )
				{
					case 123:
						return objectOne;
					case 456:
						return objectTwo;
					default:
						Assert.Fail("Unexpected property retrieval");
						throw new InvalidOperationException();
				}
			};

			// Output is expected.
			var input = "resolves to: %PROPERTY_123.PROPERTY_456.PROPERTY_789%";
			Assert.AreEqual("resolves to: hello world", root.ExpandSimpleConcatenation(input, getPropertyText, getDirectReference));
		}
		
		[TestMethod]
		public void IndirectPropertyReferencePlaceholderAliases()
		{
			// Create our mock objects.
			var vaultMock = this.GetVaultMock
			(
				new Tuple<string, int>("first", 123),
				new Tuple<string, int>("second", 456),
				new Tuple<string, int>("last", 789)
			);
			var objectTwo = new Common.ObjVerEx
			(
				vaultMock.Object,
				this.GetObjectVersionAndPropertiesMock(vaultMock, 0, 456).Object
			);
			var objectOne = new Common.ObjVerEx
			(
				vaultMock.Object,
				this.GetObjectVersionAndPropertiesMock(vaultMock, 0, 123).Object
			);
			var root = new Common.ObjVerEx
			(
				vaultMock.Object,
				this.GetObjectVersionAndPropertiesMock(vaultMock, 0, 1).Object
			);

			// Create our methods for passing data around.
			Func<Common.ObjVerEx, int, string> getPropertyText = (Common.ObjVerEx o, int id) =>
			{
				switch( id )
				{
					case 789:
						return "hello world";
					default:
						Assert.Fail("Unexpected property retrieval");
						throw new InvalidOperationException();
				}
			};
			Func<Common.ObjVerEx, int, Common.ObjVerEx> getDirectReference = (Common.ObjVerEx o, int id) =>
			{
				switch( id )
				{
					case 123:
						return objectOne;
					case 456:
						return objectTwo;
					default:
						Assert.Fail("Unexpected property retrieval");
						throw new InvalidOperationException();
				}
			};

			// Output is expected.
			var input = "resolves to: %PROPERTY_{first}.PROPERTY_{second}.PROPERTY_{last}%";
			Assert.AreEqual("resolves to: hello world", root.ExpandSimpleConcatenation(input, getPropertyText, getDirectReference));
		}
		
		[TestMethod]
		public void IndirectPropertyReferencePlaceholderMixture()
		{
			// Create our mock objects.
			var vaultMock = this.GetVaultMock
			(
				new Tuple<string, int>("first", 123),
				new Tuple<string, int>("last", 789)
			);
			var objectTwo = new Common.ObjVerEx
			(
				vaultMock.Object,
				this.GetObjectVersionAndPropertiesMock(vaultMock, 0, 456).Object
			);
			var objectOne = new Common.ObjVerEx
			(
				vaultMock.Object,
				this.GetObjectVersionAndPropertiesMock(vaultMock, 0, 123).Object
			);
			var root = new Common.ObjVerEx
			(
				vaultMock.Object,
				this.GetObjectVersionAndPropertiesMock(vaultMock, 0, 1).Object
			);

			// Create our methods for passing data around.
			Func<Common.ObjVerEx, int, string> getPropertyText = (Common.ObjVerEx o, int id) =>
			{
				switch( id )
				{
					case 789:
						return "hello world";
					default:
						Assert.Fail("Unexpected property retrieval");
						throw new InvalidOperationException();
				}
			};
			Func<Common.ObjVerEx, int, Common.ObjVerEx> getDirectReference = (Common.ObjVerEx o, int id) =>
			{
				switch( id )
				{
					case 123:
						return objectOne;
					case 456:
						return objectTwo;
					default:
						Assert.Fail("Unexpected property retrieval");
						throw new InvalidOperationException();
				}
			};

			// Output is expected.
			var input = "resolves to: %PROPERTY_{first}.PROPERTY_456.PROPERTY_{last}%";
			Assert.AreEqual("resolves to: hello world", root.ExpandSimpleConcatenation(input, getPropertyText, getDirectReference));
		}
		
		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void InvalidPropertyDefinitionThrows()
		{
			// Create our mock objects.
			var vaultMock = this.GetVaultMock();
			var root = new Common.ObjVerEx
			(
				vaultMock.Object,
				this.GetObjectVersionAndPropertiesMock(vaultMock, 0, 1).Object
			);

			// Create our methods for passing data around.
			Func<Common.ObjVerEx, int, string> getPropertyText = (Common.ObjVerEx o, int id) =>
			{
				Assert.Fail("Unexpected property retrieval");
				throw new InvalidOperationException();
			};
			Func<Common.ObjVerEx, int, Common.ObjVerEx> getDirectReference = (Common.ObjVerEx o, int id) =>
			{
				Assert.Fail("Unexpected property retrieval");
				throw new InvalidOperationException();
			};

			// Output is expected.
			// Note: property definition with alias "first" is not found in the vault, so an exception is expected.
			var input = "resolves to: %PROPERTY_{first}.PROPERTY_456.PROPERTY_{last}%";
			root.ExpandSimpleConcatenation(input, getPropertyText, getDirectReference);
		}
		
		[TestMethod]
		public void IndirectPropertyReferencePlaceholderNullReference()
		{
			// Create our mock objects.
			var vaultMock = this.GetVaultMock
			(
				new Tuple<string, int>("first", 123),
				new Tuple<string, int>("last", 789)
			);
			var root = new Common.ObjVerEx
			(
				vaultMock.Object,
				this.GetObjectVersionAndPropertiesMock(vaultMock, 0, 1).Object
			);

			// Create our methods for passing data around.
			Func<Common.ObjVerEx, int, string> getPropertyText = (Common.ObjVerEx o, int id) =>
			{
				Assert.Fail("Unexpected property retrieval");
				throw new InvalidOperationException();
			};
			Func<Common.ObjVerEx, int, Common.ObjVerEx> getDirectReference = (Common.ObjVerEx o, int id) =>
			{
				switch( id )
				{
					case 123:
						return null;
					default:
						Assert.Fail("Unexpected property retrieval");
						throw new InvalidOperationException();
				}
			};

			// Output is expected.
			// Note: property "first" returns null, as if the lookup were empty.
			// So the system should stop at this point and return an empty string.
			// BUT the other tokens (e.g. EXTERNALID) should still be resolved.
			var input = "resolves to: %PROPERTY_{first}.PROPERTY_456.PROPERTY_{last}% (%EXTERNALID%)";
			Assert.AreEqual("resolves to:  (123ABCDEF123)", root.ExpandSimpleConcatenation(input, getPropertyText, getDirectReference));
		}

		protected virtual Mock<Vault> GetVaultMock(params Tuple<string, int>[] propertyDefinitions)
		{
			var propertyDefOperationsMock = new Mock<VaultPropertyDefOperations>();
			propertyDefOperationsMock
				.Setup(o => o.GetPropertyDefIDByAlias(It.IsAny<string>()))
				.Returns((string alias) =>
				{
					return propertyDefinitions
						.FirstOrDefault(t => t.Item1 == alias)?
						.Item2 ?? -1;
				});

			var vault = base.GetVaultMock();
			vault.Setup(v => v.PropertyDefOperations).Returns(propertyDefOperationsMock.Object);
			return vault;
		}

		protected virtual Mock<ObjectVersionAndProperties> GetObjectVersionAndPropertiesMock
			(
			Mock<Vault> vaultMock,
			int objectTypeId = 0,
			int objectId = 123,
			int version = 1,
			string externalId = "123ABCDEF123",
			params Tuple<int, MFDataType, object>[] propertyValues
			)
		{
			var objectVersionAndPropertiesMock = new Mock<ObjectVersionAndProperties>();
			objectVersionAndPropertiesMock
				.Setup(o => o.ObjVer)
				.Returns(() =>
				{
					var objVer = new ObjVer();
					objVer.SetIDs(objectTypeId, objectId, version);
					return objVer;
				});
			objectVersionAndPropertiesMock
				.Setup(o => o.VersionData)
				.Returns(() =>
				{
					var data = new Mock<ObjectVersion>();
					data.Setup(o => o.DisplayIDAvailable).Returns(true);
					data.Setup(o => o.DisplayID).Returns(externalId);
					return data.Object;
				});
			objectVersionAndPropertiesMock
				.Setup(o => o.Properties)
				.Returns(() =>
				{
					var data = new PropertyValues();
					foreach(var tuple in propertyValues ?? new Tuple<int, MFDataType, object>[0])
					{
						var pv = new PropertyValue { PropertyDef = tuple.Item1 };
						pv.Value.SetValue(tuple.Item2, tuple.Item3);
						data.Add(-1, pv);
					}
					return data;
				});
			return objectVersionAndPropertiesMock;
		}
	}
}
