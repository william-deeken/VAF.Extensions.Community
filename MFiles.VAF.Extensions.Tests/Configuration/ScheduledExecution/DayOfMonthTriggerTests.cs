using MFiles.VAF.Extensions;
using MFiles.VAF.Extensions.Configuration.ScheduledExecution;
using MFiles.VAF.Extensions.ScheduledExecution;
using MFiles.VAF.Extensions.Tests.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Tests.ScheduledExecution
{
	[TestClass]
	[DataMemberRequired(nameof(DayOfMonthTrigger.TriggerTimes), nameof(DayOfMonthTrigger.UnrepresentableDateHandling))]
	[JsonConfEditorRequired(nameof(DailyTrigger.TriggerTimes), ChildTypeEditor = "time")]
	public class DayOfMonthTriggerTests
		: ConfigurationClassTestBase<DayOfMonthTrigger>
	{
		[TestMethod]
		[DynamicData(nameof(GetNextDayOfMonthData), DynamicDataSourceType.Method)]
		public void GetNextDayOfMonth
		(
			DateTimeOffset after,
			int dayOfMonth,
			UnrepresentableDateHandling unrepresentableDateHandling,
			DateTimeOffset?[] expected,
			DayOfMonthTriggerType triggerType = DayOfMonthTriggerType.SpecificDate,
			int nthWeekday = -1,
			DayOfWeek dayOfWeek = DayOfWeek.Sunday
		)
		{
			DateTimeOffset[] result;
			if (triggerType == DayOfMonthTriggerType.SpecificDate)
			{
				result = DayOfMonthTrigger
				.GetNextDayOfMonth(after, dayOfMonth, unrepresentableDateHandling)?
				.ToArray();
			}
			else
			{
				result = DayOfMonthTrigger
				.GetNextDayOfMonth(after, dayOfMonth, unrepresentableDateHandling, triggerType, nthWeekday, dayOfWeek)?
				.ToArray();
			}

			Assert.IsNotNull(result);
			Assert.AreEqual(expected.Length, result.Length);
			for (var i = 0; i < result.Length; i++)
			{
				Assert.AreEqual(expected[i], result[i]);
			}
		}

		[TestMethod]
		[DynamicData(nameof(GetNextExecutionData), DynamicDataSourceType.Method)]
		public void GetNextExecution
		(
			IEnumerable<TimeSpan> triggerTimes,
			IEnumerable<int> triggerDays,
			DateTimeOffset? after,
			DateTimeOffset? expected
		)
		{
			var execution = new DayOfMonthTrigger()
			{
				TriggerTimes = triggerTimes.ToList(),
				TriggerDays = triggerDays.ToList()
			}.GetNextExecution(after, TimeZoneInfo.Utc);
			Assert.AreEqual(expected?.ToUniversalTime(), execution?.ToUniversalTime());
		}
		[TestMethod]
		[DynamicData(nameof(GetVariableDateOfMonthData), DynamicDataSourceType.Method)]
		public void GetVariableDateOfMonth
		(
			DateTimeOffset date,
			int nthWeekday,
			DayOfWeek dayOfWeek,
			int expected
		)
		{
			var result = DayOfMonthTrigger.GetVariableDateOfMonth(
				date, nthWeekday, dayOfWeek);
			Assert.IsNotNull(result);
			Assert.AreEqual(expected, result);

		}
		[TestMethod]
		[DynamicData(nameof(GetNextMonthWithValidVariableDateData), DynamicDataSourceType.Method)]
		public void GetNextMonthWithValidVariableDate
		(
			DateTimeOffset date,
			UnrepresentableDateHandling unrepresentableDateHandling,
			int nthWeekday,
			DayOfWeek dayOfWeek,
			DateTimeOffset? expected
		)
		{
			var result = DayOfMonthTrigger.GetNextMonthWithValidVariableDate(
				date, unrepresentableDateHandling, nthWeekday, dayOfWeek);
			Assert.IsNotNull(result);
			Assert.AreEqual(expected, result);

		}
		public static IEnumerable<object[]> GetNextExecutionData()
		{
			// Execution later same day.
			yield return new object[]
			{
				new []{ new TimeSpan(17, 0, 0) },
				new []{ 17 }, // 5pm on the 17th
				new DateTimeOffset(2021, 03, 17, 01, 00, 00, TimeSpan.Zero), // 17th @ 1am
				new DateTimeOffset(2021, 03, 17, 17, 00, 00, TimeSpan.Zero), // 17th @ 5pm
			};

			// Execution later same day (one passed).
			yield return new object[]
			{
				new []{ new TimeSpan(0, 0, 0), new TimeSpan(17, 0, 0) },
				new []{ 17 },
				new DateTimeOffset(2021, 03, 17, 01, 00, 00, TimeSpan.Zero), // 17th @ 1am
				new DateTimeOffset(2021, 03, 17, 17, 00, 00, TimeSpan.Zero), // 17th @ 5pm
			};

			// Execution later same day (multiple matching, returns first).
			yield return new object[]
			{
				new []{ new TimeSpan(14, 0, 0), new TimeSpan(17, 0, 0) },
				new []{ 17 },
				new DateTimeOffset(2021, 03, 17, 01, 00, 00, TimeSpan.Zero), // 17th @ 1am
				new DateTimeOffset(2021, 03, 17, 14, 00, 00, TimeSpan.Zero), // 17th @ 2pm
			};

			// Execution next day.
			yield return new object[]
			{
				new []{ new TimeSpan(14, 0, 0) },
				new []{ 18 },
				new DateTimeOffset(2021, 03, 17, 01, 00, 00, TimeSpan.Zero), // 17th @ 1am
				new DateTimeOffset(2021, 03, 18, 14, 00, 00, TimeSpan.Zero), // 18th @ 2pm
			};

			// Execution next day (multiple matching, returns first).
			yield return new object[]
			{
				new []{ new TimeSpan(0, 0, 0), new TimeSpan(17, 0, 0) },
				new []{ 18 },
				new DateTimeOffset(2021, 03, 17, 01, 00, 00, TimeSpan.Zero), // 17th @ 1am
				new DateTimeOffset(2021, 03, 18, 00, 00, 00, TimeSpan.Zero), // 18th @ midnight
			};

			// Execution next month.
			yield return new object[]
			{
				new []{ new TimeSpan(17, 0, 0) },
				new []{ 16 },
				new DateTimeOffset(2021, 04, 17, 01, 00, 00, TimeSpan.Zero), // 17th April @ 1am
				new DateTimeOffset(2021, 05, 16, 17, 00, 00, TimeSpan.Zero), // 16th May @ 5pm
			};

			// Execution next week (multiple days matching, first).
			yield return new object[]
			{
				new []{ new TimeSpan(0, 0, 0) },
				new []{ 14, 16 },
				new DateTimeOffset(2021, 04, 17, 01, 00, 00, TimeSpan.Zero), // 17th April @ 1am
				new DateTimeOffset(2021, 05, 14, 00, 00, 00, TimeSpan.Zero), // 14th May @ 5pm
			};

			// Execution next week (one day this week passed, returns next week's execution).
			yield return new object[]
			{
				new []{ new TimeSpan(2, 0, 0) },
				new []{ 16, 20 },
				new DateTimeOffset(2021, 04, 17, 03, 00, 00, TimeSpan.Zero), // 17th @ 3am
				new DateTimeOffset(2021, 04, 20, 02, 00, 00, TimeSpan.Zero), // 20th @ 5pm
			};

			// No valid executions = null.
			yield return new object[]
			{
				new TimeSpan[0],
				new [] { 20 },
				new DateTimeOffset(2021, 03, 17, 18, 00, 00, TimeSpan.Zero), // Wednesday @ 6pm
				(DateTimeOffset?)null
			};

			// Exact current time returns next month.
			yield return new object[]
			{
				new []{ new TimeSpan(2, 0, 0) },
				new []{ 17 },
				new DateTimeOffset(2021, 04, 17, 02, 00, 00, TimeSpan.Zero),
				new DateTimeOffset(2021, 05, 17, 02, 00, 00, TimeSpan.Zero),
			};
		}

		public static IEnumerable<object[]> GetNextDayOfMonthData()
		{
			// Today is returned as today and next week.
			yield return new object[]
			{
				new DateTimeOffset(2021, 03, 17, 0, 0, 0, 0, TimeSpan.Zero), // Wednesday
				17, // Get the 17th
				UnrepresentableDateHandling.Skip,
				new DateTimeOffset?[]
				{
					new DateTimeOffset(2021, 03, 17, 0, 0, 0, 0, TimeSpan.Zero), // It should return the same day.
					new DateTimeOffset(2021, 04, 17, 0, 0, 0, 0, TimeSpan.Zero), // It should return next month too.
				}
			};

			// 17th and want next 18th.
			yield return new object[]
			{
				new DateTimeOffset(2021, 03, 17, 0, 0, 0, 0, TimeSpan.Zero),
				18,
				UnrepresentableDateHandling.Skip,
				new DateTimeOffset?[] { new DateTimeOffset(2021, 03, 18, 0, 0, 0, 0, TimeSpan.Zero) }
			};

			// 17th and want 16th.
			yield return new object[]
			{
				new DateTimeOffset(2021, 03, 17, 0, 0, 0, 0, TimeSpan.Zero),
				16,
				UnrepresentableDateHandling.Skip,
				new DateTimeOffset?[] { new DateTimeOffset(2021, 04, 16, 0, 0, 0, 0, TimeSpan.Zero) }
			};

			// Invalid day of the month.
			yield return new object[]
			{
				new DateTimeOffset(2021, 03, 17, 0, 0, 0, 0, TimeSpan.Zero),
				-1,
				UnrepresentableDateHandling.Skip,
				new DateTimeOffset?[0]
			};
			yield return new object[]
			{
				new DateTimeOffset(2021, 03, 17, 0, 0, 0, 0, TimeSpan.Zero),
				45,
				UnrepresentableDateHandling.Skip,
				new DateTimeOffset?[0]
			};

			// 30th of February -> 30th March (no valid day in Feb).
			yield return new object[]
			{
				new DateTimeOffset(2021, 02, 17, 0, 0, 0, 0, TimeSpan.Zero),
				30,
				UnrepresentableDateHandling.Skip,
				new DateTimeOffset?[] { new DateTimeOffset(2021, 03, 30, 0, 0, 0, 0, TimeSpan.Zero) }
			};

			// 30th of February -> 28th Feb (no valid day in Feb).
			yield return new object[]
			{
				new DateTimeOffset(2021, 02, 17, 0, 0, 0, 0, TimeSpan.Zero),
				30,
				UnrepresentableDateHandling.LastDayOfMonth,
				new DateTimeOffset?[] { new DateTimeOffset(2021, 02, 28, 0, 0, 0, 0, TimeSpan.Zero) }
			};

			// 30th of February -> 29th Feb (leap year, and no valid day in Feb).
			yield return new object[]
			{
				new DateTimeOffset(2024, 02, 17, 0, 0, 0, 0, TimeSpan.Zero),
				30,
				UnrepresentableDateHandling.LastDayOfMonth,
				new DateTimeOffset?[] { new DateTimeOffset(2024, 02, 29, 0, 0, 0, 0, TimeSpan.Zero) }
			};
			// Variable Day: -1st Saturday of March -> Invalid
			//yield return new object[]
			//{
			//	new DateTimeOffset(2025, )
			//}
		}
		public static IEnumerable<object[]> GetVariableDateOfMonthData()
		{
			// -1st Wednesday (Invalid) -> -1
			yield return new object[]
			{
				new DateTimeOffset(2025, 2, 23, 1, 2, 3, TimeSpan.Zero),
				-1,
				DayOfWeek.Wednesday,
				-1
			};
			// 6th Thursday (Invalid) -> -1
			yield return new object[]
			{
				new DateTimeOffset(2025, 2, 23, 1, 2, 3, TimeSpan.Zero),
				6,
				DayOfWeek.Thursday,
				-1
			};
			// 1st Friday of Jan 2025 -> 3
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 23, 1, 2, 3, TimeSpan.Zero),
				1,
				DayOfWeek.Friday,
				3
			};
			// 1st Wednesday of Jan 2025 (1st of the month) -> 1
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 23, 1, 2, 3, TimeSpan.Zero),
				1,
				DayOfWeek.Wednesday,
				1
			};
			// 2nd Thursday of Jan 2025 -> 9
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 3, 1, 2, 3, TimeSpan.Zero),
				2,
				DayOfWeek.Thursday,
				9
			};
			// 2nd Wednesday of Jan 2025 -> 8
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 13, 1, 2, 3, TimeSpan.Zero),
				2,
				DayOfWeek.Wednesday,
				8
			};
			// 1st Tuesday of Jan 2025 -> 7
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 13, 1, 2, 3, TimeSpan.Zero),
				1,
				DayOfWeek.Tuesday,
				7
			};
			// 4th Monday of Jan 2025 -> 27
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 13, 1, 2, 3, TimeSpan.Zero),
				4,
				DayOfWeek.Monday,
				27
			};
			// 5th Wednesday of Jan 2025 -> 27
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 13, 1, 2, 3, TimeSpan.Zero),
				5,
				DayOfWeek.Wednesday,
				29
			};
			// 5th Friday of Jan 2025 (Last day of month) -> 31
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 13, 1, 2, 3, TimeSpan.Zero),
				5,
				DayOfWeek.Friday,
				31
			};
			// 5th Saturday of Jan 2025 (Does not exist) -> -1
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 13, 1, 2, 3, TimeSpan.Zero),
				5,
				DayOfWeek.Saturday, 
				-1
			};
			// 5th Monday of Jan 2025 (Does not exist) -> -1
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 13, 1, 2, 3, TimeSpan.Zero),
				5,
				DayOfWeek.Monday, 
				-1
			};
			// Feb Testing: 4th Friday of Feb 2025 -> 28
			yield return new object[]
			{
				new DateTimeOffset(2025, 2, 13, 1, 2, 3, TimeSpan.Zero),
				4,
				DayOfWeek.Friday, 
				28
			};
			// Feb Testing: 4th Saturday of Feb 2025 -> 22
			yield return new object[]
			{
				new DateTimeOffset(2025, 2, 13, 1, 2, 3, TimeSpan.Zero),
				4,
				DayOfWeek.Saturday,
				22
			};
			// Feb Testing: 5th Saturday of Feb 2025 (Does not exist) -> -1 
			yield return new object[]
			{
				new DateTimeOffset(2025, 2, 13, 1, 2, 3, TimeSpan.Zero),
				5,
				DayOfWeek.Saturday,
				-1
			};
			// Feb Testing LEAP YEAR: 4th Monday of Feb 2028-> 28
			yield return new object[]
			{
				new DateTimeOffset(2028, 2, 13, 1, 2, 3, TimeSpan.Zero),
				4,
				DayOfWeek.Monday,
				28
			};
			// Feb Testing LEAP YEAR: 5th Tuesday of Feb 2028-> 29
			yield return new object[]
			{
				new DateTimeOffset(2028, 2, 13, 1, 2, 3, TimeSpan.Zero),
				5,
				DayOfWeek.Tuesday,
				29
			};
			// End of year: 5th Wed of Dec 2025 -> 31
			yield return new object[]
			{
				new DateTimeOffset(2025, 12, 13, 1, 2, 3, TimeSpan.Zero),
				5,
				DayOfWeek.Wednesday,
				31
			};
			// End of year: 5th Thursday of Dec 2025 (DNE) -> -1
			yield return new object[]
			{
				new DateTimeOffset(2025, 12, 13, 1, 2, 3, TimeSpan.Zero),
				5,
				DayOfWeek.Thursday,
				-1
			};

		}
		public static IEnumerable<object[]> GetNextMonthWithValidVariableDateData()
		{
			// Unrepresentable = SKIP - Jan 2025 - 1st Saturday -> 1 Feb 2025 
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 1, 1, 2, 3, TimeSpan.Zero),
				UnrepresentableDateHandling.Skip,
				1,
				DayOfWeek.Saturday,
				new DateTimeOffset(2025, 2, 1, 0, 0, 0, TimeSpan.Zero)
			};
			// Unrepresentable = SKIP - Jan 2025 - 1st Sunday -> 2 Feb 2025
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 1, 1, 2, 3, TimeSpan.Zero),
				UnrepresentableDateHandling.Skip,
				1,
				DayOfWeek.Sunday,
				new DateTimeOffset(2025, 2, 2, 0, 0, 0, TimeSpan.Zero)
			};
			// Unrepresentable = SKIP - Jan 2025 - 2ND Sunday -> 9 Feb 2025
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 1, 1, 2, 3, TimeSpan.Zero),
				UnrepresentableDateHandling.Skip,
				2,
				DayOfWeek.Sunday,
				new DateTimeOffset(2025, 2, 9, 0, 0, 0, TimeSpan.Zero)
			};
			// Unrepresentable = SKIP - Jan 2025 - 3ND Sunday -> 16 Feb 2025
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 1, 1, 2, 3, TimeSpan.Zero),
				UnrepresentableDateHandling.Skip,
				3,
				DayOfWeek.Sunday,
				new DateTimeOffset(2025, 2, 16, 0, 0, 0, TimeSpan.Zero)
			};
			// Unrepresentable = SKIP - Jan 2025 - 4TH Sunday -> 23 Feb 2025
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 1, 1, 2, 3, TimeSpan.Zero),
				UnrepresentableDateHandling.Skip,
				4,
				DayOfWeek.Sunday,
				new DateTimeOffset(2025, 2, 23, 0, 0, 0, TimeSpan.Zero)
			};
			// Unrepresentable = SKIP - Jan 2025 - 4TH Friday -> 28 Feb 2025
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 1, 1, 2, 3, TimeSpan.Zero),
				UnrepresentableDateHandling.Skip,
				4,
				DayOfWeek.Friday,
				new DateTimeOffset(2025, 2, 28, 0, 0, 0, TimeSpan.Zero)
			};
			// Unrepresentable = SKIP - Jan 2025 - 5TH Saturday (Invalid, Skip Feb) -> 29 March 2025
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 1, 1, 2, 3, TimeSpan.Zero),
				UnrepresentableDateHandling.Skip,
				5,
				DayOfWeek.Saturday,
				new DateTimeOffset(2025, 3, 29, 0, 0, 0, TimeSpan.Zero)
			};
			// Unrepresentable = SKIP - Jan 2025 - 5TH Wednesday (Invalid, Skip Feb AND Skip Mar) -> 30 April 2025
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 1, 1, 2, 3, TimeSpan.Zero),
				UnrepresentableDateHandling.Skip,
				5,
				DayOfWeek.Wednesday,
				new DateTimeOffset(2025, 4, 30, 0, 0, 0, TimeSpan.Zero)
			};
			// Unrepresentable = EndOfMonth - Jan 2025 - 5TH Saturday (Invalid, use end of month) -> 28 Feb 2025
			yield return new object[]
			{
				new DateTimeOffset(2025, 1, 1, 1, 2, 3, TimeSpan.Zero),
				UnrepresentableDateHandling.LastDayOfMonth,
				5,
				DayOfWeek.Saturday,
				new DateTimeOffset(2025, 2, 28, 0, 0, 0, TimeSpan.Zero)
			};
			// LEAP YEAR - Unrepresentable = SKIP - Jan 2028 - 5th Tuesday -> 29 Feb 2028
			yield return new object[]
			{
				new DateTimeOffset(2028, 1, 1, 1, 2, 3, TimeSpan.Zero),
				UnrepresentableDateHandling.Skip,
				5,
				DayOfWeek.Tuesday,
				new DateTimeOffset(2028, 2, 29, 0, 0, 0, TimeSpan.Zero)
			};
			// LEAP YEAR - Unrepresentable = SKIP - Jan 2028 - 5th Wednesday (DNE)-> 29 Mar 2028
			yield return new object[]
			{
				new DateTimeOffset(2028, 1, 1, 1, 2, 3, TimeSpan.Zero),
				UnrepresentableDateHandling.Skip,
				5,
				DayOfWeek.Wednesday,
				new DateTimeOffset(2028, 3, 29, 0, 0, 0, TimeSpan.Zero)
			};


		}
	}
}
