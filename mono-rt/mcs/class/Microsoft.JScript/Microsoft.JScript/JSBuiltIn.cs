//
// JSBuiltIn.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

namespace Microsoft.JScript {

	public enum JSBuiltin {
		Array_concat = 1,
		Array_join,
		Array_pop,
		Array_push,
		Array_reverse,
		Array_shift,
		Array_slice,
		Array_sort,
		Array_splice,
		Array_toLocaleString,
		Array_toString,
		Array_unshift,
		Boolean_toString,
		Boolean_valueOf,
		Date_getDate,
		Date_getDay,
		Date_getFullYear,
		Date_getHours,
		Date_getMilliseconds,
		Date_getMinutes,
		Date_getMonth,
		Date_getSeconds,
		Date_getTime,
		Date_getTimezoneOffset,
		Date_getUTCDate,
		Date_getUTCDay,
		Date_getUTCFullYear,
		Date_getUTCHours,
		Date_getUTCMilliseconds,
		Date_getUTCMinutes,
		Date_getUTCMonth,
		Date_getUTCSeconds,
		Date_getVarDate,
		Date_getYear,
		Date_parse,
		Date_setDate,
		Date_setFullYear,
		Date_setHours,
		Date_setMinutes,
		Date_setMilliseconds,
		Date_setMonth,
		Date_setSeconds,
		Date_setTime,
		Date_setUTCDate,
		Date_setUTCFullYear,
		Date_setUTCHours,
		Date_setUTCMinutes,
		Date_setUTCMilliseconds,
		Date_setUTCMonth,
		Date_setUTCSeconds,
		Date_setYear,
		Date_toDateString,
		Date_toGMTString,
		Date_toLocaleDateString,
		Date_toLocaleString,
		Date_toLocaleTimeString,
		Date_toString,
		Date_toTimeString,
		Date_toUTCString,
		Date_UTC,
		Date_valueOf,
		Enumerator_atEnd,
		Enumerator_item,
		Enumerator_moveFirst,
		Enumerator_moveNext,
		Error_toString,
		Function_apply,
		Function_call,
		Function_toString,
		Global_CollectGarbage,
		Global_decodeURI,
		Global_decodeURIComponent,
		Global_encodeURI,
		Global_encodeURIComponent,
		Global_escape,
		Global_eval,
		Global_GetObject,
		Global_isNaN,
		Global_isFinite,
		Global_parseFloat,
		Global_parseInt,
		Global_ScriptEngine,
		Global_ScriptEngineBuildVersion,
		Global_ScriptEngineMajorVersion,
		Global_ScriptEngineMinorVersion,
		Global_unescape,
		Math_abs,
		Math_acos,
		Math_asin,
		Math_atan,
		Math_atan2,
		Math_ceil,
		Math_cos,
		Math_exp,
		Math_floor,
		Math_log,
		Math_max,
		Math_min,
		Math_pow,
		Math_random,
		Math_round,
		Math_sin,
		Math_sqrt,
		Math_tan,
		Number_toExponential,
		Number_toFixed,
		Number_toLocaleString,
		Number_toPrecision,
		Number_toString,
		Number_valueOf,
		Object_hasOwnProperty,
		Object_isPrototypeOf,
		Object_propertyIsEnumerable,
		Object_toLocaleString,
		Object_toString,
		Object_valueOf,
		RegExp_compile,
		RegExp_exec,
		RegExp_test,
		RegExp_toString,
		String_anchor,
		String_big,
		String_blink,
		String_bold,
		String_charAt,
		String_charCodeAt,
		String_concat,
		String_fixed,
		String_fontcolor,
		String_fontsize,
		String_fromCharCode,
		String_indexOf,
		String_italics,
		String_lastIndexOf,
		String_link,
		String_localeCompare,
		String_match,
		String_replace,
		String_search,
		String_slice,
		String_small,
		String_split,
		String_strike,
		String_sub,
		String_substr,
		String_substring,
		String_sup,
		String_toLocaleLowerCase,
		String_toLocaleUpperCase,
		String_toLowerCase,
		String_toString,
		String_toUpperCase,
		String_valueOf,
		VBArray_dimensions,
		VBArray_getItem,
		VBArray_lbound,
		VBArray_toArray,
		VBArray_ubound
	}
}
