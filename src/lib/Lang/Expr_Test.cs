using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace lib.Lang
{
	[TestFixture]
	public class Expr_Test
	{
		[Test]
		public void Tokenize()
		{
			CollectionAssert.AreEqual(new[] { "(", "x", ")" }, Expr.Tokenize("(x)"));
			CollectionAssert.AreEqual(new[] { "x" }, Expr.Tokenize("x"));
			CollectionAssert.AreEqual(new[] { "(", ")" }, Expr.Tokenize("()"));
			CollectionAssert.AreEqual(new[] { "(", ")" }, Expr.Tokenize("( )"));
			CollectionAssert.AreEqual(new[] { "(", ")" }, Expr.Tokenize("(\t\r\n)"));
			CollectionAssert.AreEqual(new[] { "(", "x", ")" }, Expr.Tokenize("(\t\r\n x)"));
			CollectionAssert.AreEqual(new[] { "(", "lambda", "(", "x", ")", "(", "shl1", "x", ")", ")" }, Expr.Tokenize("(lambda (x) (shl1 x))"));

		}

		[Test]
		public void Parse()
		{
			Assert.AreEqual("0", Expr.ParseExpr("0").ToSExpr());
			Assert.AreEqual("(shl1 1)", Expr.ParseExpr(" ( shl1      1) ").ToSExpr());
			Assert.AreEqual("(fold 0 0 (lambda (x i) (shl1 x)))", Expr.ParseExpr("(fold 0 0 (lambda (x   i )  ( shl1  x ) ))").ToSExpr());
		}

		[TestCase("0")]
		[TestCase("1")]
		[TestCase("(shl1 1)")]
		[TestCase("(shr1 1)")]
		[TestCase("(shr4 1)")]
		[TestCase("(shr16 1)")]
		[TestCase("(not 1)")]
		[TestCase("(and 1 0)")]
		[TestCase("(or 1 0)")]
		[TestCase("(xor 1 0)")]
		[TestCase("(plus 1 0)")]
		[TestCase("(if0 0 1 0)")]
		[TestCase("(fold 0 1 (lambda (x y) (plus x y)))")]
		[TestCase("(xor (fold 0 1 (lambda (x y) (plus x y))) 1)")]
		public void ParseCorrect(string s)
		{
			Assert.AreEqual(s, Expr.ParseExpr(s).ToSExpr());
		}

		[Test]
		[TestCaseSource("GetEvalTestCases")]
		public void Eval(TestCase t)
		{
			var actual = Expr.Eval(t.Program, t.Arg);
			Assert.That(actual, Is.EqualTo(t.ExpectedValue), string.Format("Was: 0x{0:x}", actual));
		}

		[Test]
		[TestCaseSource("GetEvalTestCases")]
		public void EvalBin(TestCase t)
		{
			byte[] p = Parser.ParseFunction(t.Program);
			var actual = p.Eval(t.Arg);
			Assert.That(actual, Is.EqualTo(t.ExpectedValue), string.Format("Was: 0x{0:x}", actual));
		}
		
		private static IEnumerable<TestCase> GetEvalTestCases()
		{
			yield return new TestCase("(lambda (x) 0)", 1, 0);
			yield return new TestCase("(lambda (x) 0)", 0, 0);
			yield return new TestCase("(lambda (x) 1)", 1, 1);
			yield return new TestCase("(lambda (x) 1)", 0, 1);

			yield return new TestCase("(lambda (x) x)", 1, 1);
			yield return new TestCase("(lambda (x) x)", 0, 0);
			yield return new TestCase("(lambda (x) x)", 5, 5);

			yield return new TestCase("(lambda (x) (not x))", 0, 0xffffffffffffffff);
			yield return new TestCase("(lambda (x) (not x))", 1, 0xfffffffffffffffe);
			yield return new TestCase("(lambda (x) (not x))", 0xffffffffffffffff, 0);
			yield return new TestCase("(lambda (x) (not x))", 0xfffffffffffffffe, 1);

			yield return new TestCase("(lambda (x) (shl1 x))", 0, 0);
			yield return new TestCase("(lambda (x) (shl1 x))", 1, 2);
			yield return new TestCase("(lambda (x) (shl1 x))", 0xffffffffffffffff, 0xfffffffffffffffe);
			yield return new TestCase("(lambda (x) (shl1 x))", 0xfffffffffffffffe, 0xfffffffffffffffc);
			yield return new TestCase("(lambda (x) (shl1 x))", 3, 6);

			yield return new TestCase("(lambda (x) (shr1 x))", 0, 0);
			yield return new TestCase("(lambda (x) (shr1 x))", 1, 0);
			yield return new TestCase("(lambda (x) (shr1 x))", 0xffffffffffffffff, 0x7fffffffffffffff);
			yield return new TestCase("(lambda (x) (shr1 x))", 0xfffffffffffffffe, 0x7fffffffffffffff);

			yield return new TestCase("(lambda (x) (shr4 x))", 0, 0);
			yield return new TestCase("(lambda (x) (shr4 x))", 1, 0);
			yield return new TestCase("(lambda (x) (shr4 x))", 0xf, 0);
			yield return new TestCase("(lambda (x) (shr4 x))", 0x10, 1);
			yield return new TestCase("(lambda (x) (shr4 x))", 0xffffffffffffffff, 0x0fffffffffffffff);

			yield return new TestCase("(lambda (x) (shr16 x))", 0, 0);
			yield return new TestCase("(lambda (x) (shr16 x))", 1, 0);
			yield return new TestCase("(lambda (x) (shr16 x))", 0xffff, 0);
			yield return new TestCase("(lambda (x) (shr16 x))", 0x10000, 1);
			yield return new TestCase("(lambda (x) (shr16 x))", 0xffffffffffffffff, 0x0000ffffffffffff);

			yield return new TestCase("(lambda (x) (and x 0))", 0, 0);
			yield return new TestCase("(lambda (x) (and x 0))", 1, 0);
			yield return new TestCase("(lambda (x) (and x 1))", 0, 0);
			yield return new TestCase("(lambda (x) (and x 1))", 1, 1);

			yield return new TestCase("(lambda (x) (or x 0))", 0, 0);
			yield return new TestCase("(lambda (x) (or x 0))", 1, 1);
			yield return new TestCase("(lambda (x) (or x 1))", 0, 1);
			yield return new TestCase("(lambda (x) (or x 1))", 1, 1);

			yield return new TestCase("(lambda (x) (xor x 0))", 0, 0);
			yield return new TestCase("(lambda (x) (xor x 0))", 1, 1);
			yield return new TestCase("(lambda (x) (xor x 1))", 0, 1);
			yield return new TestCase("(lambda (x) (xor x 1))", 1, 0);
			yield return new TestCase("(lambda (x) (xor x x))", 3, 0);
			yield return new TestCase("(lambda (x) (xor x x))", unchecked ((ulong) (-1)), 0);

			yield return new TestCase("(lambda (x) (plus x 0))", 0, 0);
			yield return new TestCase("(lambda (x) (plus x 0))", 1, 1);
			yield return new TestCase("(lambda (x) (plus x 1))", 0, 1);
			yield return new TestCase("(lambda (x) (plus x 1))", 1, 2);
			yield return new TestCase("(lambda (x) (plus x 1))", 0xfffffffffffffffe, 0xffffffffffffffff);
			yield return new TestCase("(lambda (x) (plus x 1))", 0xffffffffffffffff, 0);

			yield return new TestCase("(lambda (x) (xor 0 (and 1 (or x (plus x (not (shl1 (shr1 (shr4 (shr16 x))))))))))", 0, 1);
			yield return new TestCase("(lambda (x) (xor 0 (and x (or x (plus x (not (shl1 (shr1 (shr4 (shr16 x))))))))))", 12345678, 12345678);
			yield return new TestCase("(lambda (x) (not (shl1 (shr1 (shr4 (shr16 x))))))", 1 << 20, unchecked((ulong)(-1)));

			yield return new TestCase("(lambda (x) (fold x 0 (lambda (x acc) 0)))", 0, 0);
			yield return new TestCase("(lambda (x) (fold x 0 (lambda (x acc) 0)))", 1, 0);
			yield return new TestCase("(lambda (x) (fold x 1 (lambda (x acc) 1)))", 0, 1);
			yield return new TestCase("(lambda (x) (fold x 1 (lambda (x acc) 1)))", 1, 1);
			yield return new TestCase("(lambda (x) (fold x 0 (lambda (x acc) (plus x acc)))", 0, 0);
			yield return new TestCase("(lambda (x) (fold x 0 (lambda (x acc) (plus x acc)))", 1, 1);
			yield return new TestCase("(lambda (x) (fold x (plus 1 1) (lambda (x acc) (plus x acc)))", 65535, 512);

			yield return new TestCase("(lambda (x) (if0 0 1 0))", 1, 1);
			yield return new TestCase("(lambda (x) (if0 0 1 0))", 0, 1);
			yield return new TestCase("(lambda (x) (if0 x 0 1))", 0, 0);
			yield return new TestCase("(lambda (x) (if0 x 0 1))", 1, 1);

            // (fold x_27005 0 (lambda (x_27005 x_27006) (shr1 (if0 x_27005 (if0 (plus (shl1 (shr1 0)) x_27005) x_27005 x_27006) x_27005))))
            yield return new TestCase("(lambda (x) (plus (fold (plus x 1) 0 (lambda (x acc) (plus x acc))) x))", 1, 3);
            yield return new TestCase("(lambda (x) (plus x (fold (plus x 1) 0 (lambda (x acc) (plus x acc)))))", 1, 3);


            var inputs = new String[] {"3EAF985C3F3F598C","4D51A41C4DF43282","456DC6924B281326","6BF913091876FD06","73B2DDC448095D48","51885FA20976F1FC","3FD76D76488ED14F","7A92419132179528","73741CB71CE1071C","53B87E2E6622958D","2FA1082C321CE854","616724284716FEAA","255B4B2E5C015910","5395045A405F718E","21691A25DD6788E","7E4372C152DC03DB","2B941FC92A87D3DE","4B0660653442198B","11780FDD7891C3DB","A26E9B70AF3CC42","729B99B941AC01F0","1CB92E3163A09362","7E346E601E6C615","6FE04EA510438CE0","5537D4576182E77C","32521EAD26B43B22","5747F291594260B5","4B9CCAF60C92B008","5DD83563063AA572","2898E754696C6D81","77930ECB2B99A17B","16606B6415DC6536","49C5EA867EAC5BC6","5DE2D232154F9998","7D8870E946502806","209F854B7AE258DB","12BA1D6548C4B1EC","7386FBD36DF5063B","1563BA237DC690C5","3F33B7C423748519","6C210A6A4351777A","6B279D362093AA25","2B8459E1578F379F","7B9411495251BF13","5EEB08E73E73B05D","5669E4270B3D6A6B","4FF8DC8720BA7C35","1360BD764701F83E","2B4B968C06DCC8FB","19DAA8DC0936EB1F","2403F3E25A90B50D","12BB1BF60EE7AC51","40E36231376FC5D1","13FA1DBD0E8340A5","65BB64E15DA7C4BA","772EF5E46011A49D","4706EDAD0524623B","7D4B631634419751","4072044475CCC13E","6A580B547236B2E6","3184A7D0B91131F","366490B03F14C08C","6FE643DE15621AE0","2AE97C5471FFA12F","3563F4762C850395","42A96FAF0E86F128","63EBE7E935FCCCA5","7F7091361190555D","30965B845C3FF0E5","5FB047F37414940F","439519E26D10D0A4","6C965A310143442D","4744BA7876583F89","44367CBD4AD47A4C","236F191E5F1F2624","68FF3FA357ED54D","1C84BDA627A3F5F5","61EA0A21872E0C3","242C245D53A65B69","1F0168722B814751","C86497D21FA7C8D","591F4C2E3936614C","1AFE550B68A804BC","7C25BCB3110A2108","5B3D1046BBB0DB9","3AB3BCD6432135E","161C794B76437744","2EA19904160779D8","1EFAB8ED35214C83","77D00613798E0454","512B9E2260150207","4E9088115644CE51","25F50F9B0D2A9A62","720233813C47F1F4","2FDE2C0366FDB073","6D6430FF5CF0001A","3D3E8873342F00A2","678E4A92219A9D55","13F18476335FF8E5","6644EF215E9CB5BB","7A3282D5332C5BB5","4520A94837B40B64","5B73EA57225DE09B","1F625C0226414661","79025CF0701726C9","797827D56F0AD7D9","5BD655552573641D","5A55A92E2C714775","5369F47B02DA7DDD","134151B10DD3BAA9","76A5D13A3FDDCABF","612BF5041EB62090","7E56DCB926BB3BB2","26F38AEB61ED78A8","EB52CB20D06FBAF","215F5626B9AC007","4EDC5D6119335058","7F5B817E1DFF426D","1AF458BF16DC7CAD","7AD0E3F903972F00","6DC83E5F4BC0ED20","434595033FC70539","6D85889D7E595925","119AAC517CB2456","59D957733B1D031D","4E30A8D911170699","201EA73458713478","67F6E4803A54B816","520066B0266A88B8","395D2EAA34B8AEA5","7B6A55AF3D74E359","178C19AF6BFB6141","6E0131670DDD67CE","202E7A7842A304FD","2617E29E3C5666C1","117CEEB63C8D77CE","80999E30FAA22B1","200B7C492AE4E41A","7C9835BB2164EFCB","313097F27FFD8393","25A190ED0F286695","814213553982994","3ED652D570FEEEC8","4B7C18D3766B28E8","313C6BD74506C968","4BFEDBBC7489D006","5D7F4E0246183554","82CD95030535B06","3434D371574A33C1","51C5D3D15FA820CF","5E2ADEAD3E76A5C7","71B341B71D82F0B2","29A0146711A3310B","90D6CB610748483","3865B82F3D120066","3DBC825A309B76E5","7539F0C5395FAB16","F171DB86C41EF1A","3560C22443F3F01A","2D250E6C7D02429E","42614EFA29C3518F","1166992661111935","70578B041CF31EAF","5F0E29CB41F1648E","77D49616FB82142","76B010A8254E01FA","4F9F1BF951887722","21D2A4E6672AEB25","1D7524DD6A913082","29F8152D2D3321CA","67F182123B079450","3E0570B9742A6B70","74A470E1B6364D7","7F4FDF40241FA2EC","3701179C1BEAEA35","7AF298E17040E356","2A25255554C39132","1D46D1D534678D1E","2D100CA110A22882","2D2B65AD0CACF5B8","3295073F471C0827","8F73B2248AD96EC","4661EFBD186E8060","60FC5AEB23B179A3","178D063027EC3292","4ECCCA3D4ACF91A1","19FBDAED7FF1ECA1","1510C08C0759BAAA","6BBDE0D51D3C2DB5","59C6D22754F4262D","1DA33F6E3AEE86DF","AF04CF26B925F2B","74C58860066F2D52","7B28DCA47ADB8AC6","3441A54D6D6B17C7","3A1ADE840CD2FC5B","3D65CAD51D4B1F75","7A9E1A8A20D579EB","3210592E7557D9CC","2DC87BB36EDBC6AD","3A6709EB5B9E659D","C929CBC0F14E50A","4D1E1FF730230740","564508692A3338B3","3FB2D0A615ED172B","48A9AC480FD3DEEC","36DB565A58373F7F","6F09B64977A6805F","3C2BBB351D64DBF6","53E80E8B3FF2C26B","1D45A3BB6620D024","6F6FD4552A21EE68","6DD1540D41F9CDE2","D69C6CC7CB0BB77","553D217743B46A0","5549617B76660708","6F73B2016AEB0B79","1A8D1C424204D9D0","2E5BEA237BDB67E7","1E743F3444A28120","302A24E850F5A3F1","3B28BA201E548E22","24C16B7F2A46FF98","55F7A600652E8B55","2E41692B02F79A2B","64A9BEB514AB7D38","216FCB416DD5B947","519622F2544639C6","6C2E91476270AE53","214390FC0B245215","22B9407459944CF1","24DF669C4B778A05","40A3B5AF5243A53F","20602CEA4BF04AE0","3DAA233D2D04DC79","494A963C4050534D","7EF09CC10F458D6B","FC89D834C4FFFC9","2AF81604511B4633","56082E9438A5475","6C0227E1283B3B76","4E6F524C025C37EC","F9187EB409DE443","54F63BC7019DF8BA","1954E8874646E27B","55D648884BEB3BCF","5A97D6EB134FED1F","21E940AC5695D7F6","2C163D556FB1301A","5DB0D872527DC63F","9E6D2AE0A075B20","27846818013C8CB2","39AD03EE24596B68","15BAE076122A3DD6","5E0D1BC405462329","294E23C21B134B14"};
            var outputs = new String[] { "0000000000000001", "0000000000000000", "0000000000000000", "0000000000000002", "0000000000000003", "0000000000000000", "0000000000000001", "0000000000000003", "0000000000000003", "0000000000000000", "0000000000000000", "0000000000000002", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000003", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000003", "0000000000000000", "0000000000000000", "0000000000000002", "0000000000000000", "0000000000000001", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000003", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000003", "0000000000000000", "0000000000000000", "0000000000000003", "0000000000000000", "0000000000000001", "0000000000000002", "0000000000000002", "0000000000000000", "0000000000000003", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000002", "0000000000000003", "0000000000000000", "0000000000000003", "0000000000000000", "0000000000000002", "0000000000000000", "0000000000000001", "0000000000000002", "0000000000000000", "0000000000000001", "0000000000000000", "0000000000000002", "0000000000000003", "0000000000000001", "0000000000000000", "0000000000000000", "0000000000000002", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000003", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000003", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000003", "0000000000000000", "0000000000000002", "0000000000000001", "0000000000000002", "0000000000000000", "0000000000000002", "0000000000000003", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000003", "0000000000000003", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000003", "0000000000000002", "0000000000000003", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000003", "0000000000000000", "0000000000000003", "0000000000000002", "0000000000000000", "0000000000000002", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000002", "0000000000000000", "0000000000000001", "0000000000000003", "0000000000000000", "0000000000000002", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000003", "0000000000000001", "0000000000000000", "0000000000000000", "0000000000000001", "0000000000000000", "0000000000000001", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000001", "0000000000000000", "0000000000000000", "0000000000000003", "0000000000000000", "0000000000000000", "0000000000000001", "0000000000000001", "0000000000000003", "0000000000000000", "0000000000000001", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000003", "0000000000000000", "0000000000000000", "0000000000000003", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000002", "0000000000000001", "0000000000000000", "0000000000000003", "0000000000000001", "0000000000000003", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000001", "0000000000000000", "0000000000000000", "0000000000000002", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000002", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000003", "0000000000000003", "0000000000000001", "0000000000000001", "0000000000000001", "0000000000000003", "0000000000000001", "0000000000000000", "0000000000000001", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000001", "0000000000000000", "0000000000000001", "0000000000000002", "0000000000000001", "0000000000000000", "0000000000000000", "0000000000000002", "0000000000000002", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000002", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000001", "0000000000000001", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000002", "0000000000000000", "0000000000000000", "0000000000000002", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000001", "0000000000000000", "0000000000000003", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000002", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000000", "0000000000000001", "0000000000000000", "0000000000000000", "0000000000000000" };

		    for (int i = 0; i < inputs.Length; i++)
		    {
		        var input = Convert.ToUInt64(inputs[i], 16);
                var output = Convert.ToUInt64(outputs[i], 16);
                yield return new TestCase("(lambda (x_8547) (fold x_8547 0 (lambda (x_8547 x_8548) (shr4 (and (shr1 x_8547) x_8547)))))", input, output);
		    }
		}
        
		public class TestCase
		{
			public TestCase(string program, ulong arg, ulong expectedValue)
			{
				Program = program;
				Arg = arg;
				ExpectedValue = expectedValue;
			}

			public string Program { get; set; }
			public UInt64 Arg { get; set; }
			public UInt64 ExpectedValue { get; set; }

			public override string ToString()
			{
				return string.Format("{0}, P(0x{1:x}) = 0x{2:x}", Program, Arg, ExpectedValue);
			}
		}
	}
}