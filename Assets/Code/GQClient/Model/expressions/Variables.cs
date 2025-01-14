﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Code.GQClient.Err;
using Code.GQClient.Model.gqml;
using Code.GQClient.Model.mgmt.quests;
using Code.GQClient.Model.pages;
using UnityEngine;

namespace Code.GQClient.Model.expressions
{

    public class Variables
    {

        #region Persistence

        public static readonly string GQ_VAR_PREFIX = Definitions.GQ_PREFIX + ".var.";
        public const char VAR_TYPE_DELIMITER = ':';

        public static void SaveVariableToPrefs(string varName)
        {

            if (string.IsNullOrEmpty(varName))
            {
                Log.SignalErrorToDeveloper("Can not save variable to store without valid var name. (Ignored)");
                return;
            }

            var value = GetValue(varName);

            if (value == Value.Null)
            {
                Log.SignalErrorToDeveloper("Can not save variable ‘{0}‘ to store: value is null. (Ignored)", varName);
                return;
            }

            var storeKey = Variables.GQ_VAR_PREFIX + varName;
            var storeValue = value.AsTypedString();

            PlayerPrefs.SetString(storeKey, storeValue);
            PlayerPrefs.Save();
        }


        public static void LoadVariableFromStore(string varName)
        {
            var prefKey = Variables.GQ_VAR_PREFIX + varName;

            if (!PlayerPrefs.HasKey(prefKey))
            {
                Log.WarnAuthor("WARNING: Tried to load variable ‘{0}‘ but didn't find it in prefs.", varName);
                return;
            }

            string valueAsTypedString = PlayerPrefs.GetString(prefKey);
            Value newValue = Value.CreateValueFromTypedString(valueAsTypedString);
            SetVariableValue(varName, newValue);
        }

        #endregion


        #region Registry

        private static Dictionary<string, Value> _variables;
        private static Dictionary<string, Value> variables
        {
            get
            {
                if (_variables == null)
                {
                    _variables = new Dictionary<string, Value>();
                    _variables.Add("score", new Value(0f));
                }
                return _variables;
            }
        }

        #endregion


        #region API

        public static bool IsDefined(string varName)
        {
            return variables.ContainsKey(varName);
            // return GetValue(varName) != Value.Null;
        }

        /// <summary>
        /// Get the Value of the Variables with the specified varName or Value.Null if no such variable is found. This method will never return null.
        /// </summary>
        /// <param name="varName">Variable name.</param>
        public static Value GetValue(string varName)
        {
            if (!IsValidVariableName(varName))
            {
                Log.WarnAuthor("Assess to Variable named {0} is not possible. This is not a valid variable name.", varName);
                return Value.Null;
            }

            if (varName.StartsWith("$"))
            {
                return GetReadOnlyVariableValue(varName);
            }

            if (variables.TryGetValue(varName, out var foundValue))
            {
                return foundValue;
            }

            Log.WarnAuthor("Variable {0} was not found.", varName);
            return Value.Null;
        }

        public static void Clear(bool clearAlsoUpperCaseVariables = true)
        {
            if (clearAlsoUpperCaseVariables)
            {
                variables.Clear();
            }
            else
            {
                foreach (string key in variables.Keys.ToList())
                {
                    if (Char.IsLower(key[0]))
                    {
                        variables.Remove(key);
                    }
                }
            }
        }

        /// <summary>
        /// Stores the newValue in Variable named varName. If this variable contained a value previously that gets replaced by the newValue.
        /// </summary>
        /// <returns><c>true</c>, if variable value replaced a previously given value, <c>false</c> otherwise.</returns>
        /// <param name="varName">Variable name.</param>
        /// <param name="newValue">New value.</param>
        public static void SetVariableValue(string varName, Value newValue)
        {
            if (!IsValidUserDefinedVariableName(varName))
            {
                Log.SignalErrorToAuthor("Variable Name may not start with '$' Symbol, so you may not use {0} as you did in a SetVariable action.", varName);
                return;
            }

            SetInternalVariable(varName, newValue);
        }

        public static void SetInternalVariable(string varName, Value newValue)
        {
            var existedAlready = variables.ContainsKey(varName);
            if (existedAlready)
            {
                variables.Remove(varName);
            }

            // if right hand side is a variablename we use the value of the variable:
            if (newValue.ValType == Value.Type.VarExpression)
            {
                SetInternalVariable(varName, Variables.GetValue(newValue.AsVariableName()));
                return;
            }
            else
            {
                variables.Add(varName, newValue);
                return;
            }
        }

        #endregion


        #region Access Read Only Variables

        private static Value GetReadOnlyVariableValue(string varName)
        {
            if (varName.StartsWith(GQML.VAR_PAGE_PREFIX))
            {
                string pageIdString;
                if (varName.EndsWith(GQML.VAR_PAGE_RESULT))
                {
                    pageIdString = varName.Substring(
                        GQML.VAR_PAGE_PREFIX.Length,
                        varName.Length - (GQML.VAR_PAGE_PREFIX.Length + GQML.VAR_PAGE_RESULT.Length)
                    );
                }
                else if (varName.EndsWith(GQML.VAR_PAGE_STATE))
                {
                    pageIdString = varName.Substring(
                        GQML.VAR_PAGE_PREFIX.Length,
                        varName.Length - (GQML.VAR_PAGE_PREFIX.Length + GQML.VAR_PAGE_STATE.Length)
                    );
                }
                else
                {
                    Log.WarnAuthor("Page feature used in system variable named {0} is unknown.", varName);
                    return Value.Null;
                }

                int pageId;
                if (Int32.TryParse(pageIdString, out pageId) == false)
                {
                    Log.WarnDeveloper("Page ID {0} cannot be interpreted.", pageIdString);
                    return Value.Null;
                }
                Page page = QuestManager.Instance.CurrentQuest.GetPageWithID(pageId);
                if (page == null)
                {
                    Log.WarnDeveloper("Page with id {0} not found in quest.", pageId);
                    return Value.Null;
                }

                if (varName.EndsWith(GQML.VAR_PAGE_RESULT))
                {
                    return new Value(page.Result, Value.Type.Text);
                }

                if (varName.EndsWith(GQML.VAR_PAGE_STATE))
                {
                    return new Value(page.State, Value.Type.Text);
                }
            }

            return Value.Null;

        }

        #endregion


        #region Util Functions

        private const string VARNAME_USERDEFINED_REGEXP = @"(?!$)[a-zA-Z_]+[a-zA-Z0-9_.]*";
        private const string VARNAME_REGEXP = @"(\$?|\$_)?[a-zA-Z]+[a-zA-Z0-9_.]*";
        private const string REGEXP_START = @"^";
        private const string REGEXP_END = @"$";

        public const string UNDEFINED_VAR = "_undefined";

        public static bool IsValidUserDefinedVariableName(string name)
        {
            var regex = new Regex(REGEXP_START + VARNAME_USERDEFINED_REGEXP + REGEXP_END);
            var match = regex.Match(name);
            return match.Success;
        }

        public static bool IsValidVariableName(string name)
        {
            var regex = new Regex(REGEXP_START + VARNAME_REGEXP + REGEXP_END);
            var match = regex.Match(name);
            return match.Success;
        }

        /// <summary>
        /// Returns the longest valid name contained in the given nameCandidate starting from the beginning.
        /// </summary>
        /// <returns>The valid variable name from start.</returns>
        /// <param name="nameCandidate">Name candidate.</param>
        public static string LongestValidVariableNameFromStart(string nameCandidate)
        {
            var regex = new Regex(REGEXP_START + VARNAME_REGEXP);
            var match = regex.Match(nameCandidate);
            if (!match.Success)
                throw new ArgumentException("\"" + nameCandidate + "\" does not start with a valid variable name.");
            return match.Value;
        }

        #endregion
    }

}
