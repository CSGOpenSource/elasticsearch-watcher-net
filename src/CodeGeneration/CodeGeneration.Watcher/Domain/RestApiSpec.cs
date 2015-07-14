﻿using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CodeGeneration.Watcher.Domain
{
	public class EnumDescription
	{
		public string Name { get; set; }
		public IEnumerable<string> Options { get; set; }
	}

	public class RestApiSpec
	{
		public string Commit { get; set; }
		public IDictionary<string, ApiEndpoint> Endpoints { get; set; }

		public IList<ApiQueryParameters> ApiQueryParameters { get; set; }


		public IEnumerable<EnumDescription> EnumsInTheSpec
		{
			get
			{
				var queryParamEnums = from m in this.CsharpMethodsWithQueryStringInfo.SelectMany(m => m.Url.Params)
					where m.Value.Type == "enum"
					select new EnumDescription
					{
						Name = m.Value.CsharpType(m.Key),
						Options = m.Value.Options

					};

				var urlParamEnums = from data in this.Endpoints.Values
					.SelectMany(v =>  v.CsharpMethods.Select(m=>new { m, n = v.CsharpMethodName}))
					.SelectMany(m => m.m.Parts.Select(part=> new { m = m.n, p = part}))
					let p = data.p
					let m = data.m
					where p.Options != null && p.Options.Any()
					let name = p.Name.Contains("metric") ?  p.Name.ToPascalCase() : p.Name.ToPascalCase()
					select new EnumDescription
					{
						Name = name,
						Options = p.Options
					};

				return queryParamEnums.Concat(urlParamEnums).DistinctBy(e=>e.Name);

			}

		}


		public IEnumerable<CsharpMethod> CsharpMethodsWithQueryStringInfo
		{
			get
			{
				return (from u in this.Endpoints.Values.SelectMany(v => v.CsharpMethods)
						where u.QueryStringParamName != "FluentQueryString"
						select u).DistinctBy(m=>m.QueryStringParamName);
					
			}
		}
	}


	//extensions methods dont work because scriptcs wraps everything
	//in its own class
}
