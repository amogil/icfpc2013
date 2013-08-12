using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using lib.Web;

namespace monitor.Controllers
{
	public class HomeController : Controller
	{
		private readonly JavaScriptSerializer js = new JavaScriptSerializer();
		private readonly object locker = new object();
		private Dictionary<string, int> dictionary = new Dictionary<string, int>();
		//
		// GET: /Home/
		public ActionResult Index(string id)
		{
			if (id == null) return View();
			try
			{
				var problems = LoadProblems(id);
				return View(Tuple.Create(id.StartsWith("____")?"Unknown":id, problems));
			}
			catch (Exception e)
			{
				return Content(e.Message);
			}
		}

		[HttpPost]
		public ActionResult Post(string text)
		{
			if (text == null || text.Length > 500000) return RedirectToAction("Index");
			if (text.EndsWith("vpsH1H")) text = text.Substring(0, text.Length - 6);
			try
			{
				IList<MyProblemJson> problems;
				string shortId;
				if (text.Length == 40)
				{
					problems = new WebApi(text).MyProblems();
					shortId = StoreProblems(problems, text);
				}
				else
				{
					problems = js.Deserialize<List<MyProblemJson>>(text);
					shortId = StoreProblems(problems, null);
				}
				return RedirectToAction("Index", "Home", new {id = shortId});
			}
			catch (Exception e)
			{
				return Content(e.Message);
			}
		}

		// GET: /Stats/

		public ActionResult Stats()
		{
			lock (locker)
			{
				ReloadDic();
				return View(dictionary.OrderByDescending(kv => kv.Value).Select(kv => Tuple.Create(GetShortId(kv.Key), kv.Value)).ToList());
			}
		}

		private int TryParse(string s)
		{
			int v;
			if (int.TryParse(s, out v)) return v;
			return 0;
		}

		private string StoreProblems(IList<MyProblemJson> problems, string id)
		{
			var shortId = "";
			var serialized = js.Serialize(problems);
			if (id == null)
			{
				id = "____" + serialized.GetHashCode();
				shortId = id;
			}
			else
				shortId = id.Substring(0, 4);
			string filename = id + serialized.GetHashCode() + ".json";
			string path = Request.MapPath("~/App_Data/" + filename);
			UpdateScoreboard(problems, id);
			System.IO.File.WriteAllText(path, serialized);
			return shortId;
		}

		public static string GetShortId(string id)
		{
			if (id.StartsWith("____")) return id;
			else return id.Substring(0, 4);
		}

		private IList<MyProblemJson> LoadProblems(string id)
		{
			string path = Request.MapPath("~/App_Data/");
			var file = Directory.EnumerateFiles(path, id + "*.json").FirstOrDefault();
			if (file == null) throw new Exception(id + " not found");
			return js.Deserialize<List<MyProblemJson>>(System.IO.File.ReadAllText(file));
		}

		private void UpdateScoreboard(IList<MyProblemJson> problems, string id)
		{
			lock (locker)
			{
				string allPath = Request.MapPath("~/App_Data/all.txt");
				ReloadDic();
				dictionary[id] = problems.Count(p => p.solved == true);
				var content = dictionary.OrderByDescending(kv => kv.Value).Select(kv => kv.Key + ' ' + kv.Value);
				System.IO.File.WriteAllLines(allPath, content);
			}
		}

		private void ReloadDic()
		{
			string allPath = Request.MapPath("~/App_Data/all.txt");
			if (System.IO.File.Exists(allPath))
				dictionary = System.IO.File.ReadAllLines(allPath).Select(line => line.Split(' ')).ToDictionary(p => p[0], p => TryParse(p[1]));
			else
				dictionary = new Dictionary<string, int>();
		}
	}
}