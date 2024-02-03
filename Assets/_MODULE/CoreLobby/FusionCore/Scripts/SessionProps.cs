using System;
using System.Collections.Generic;
using System.Reflection;
using Fusion;
using UnityEngine;

namespace CoreGame
{
    [Serializable]
    public class SessionProps // or Room
    {
        public string LobbyId = string.Empty;
        public string SessionName = string.Empty;
                
        public SessionProps()
        {
        }
        
        public SessionProps(System.Collections.ObjectModel.ReadOnlyDictionary<string, SessionProperty> props)
        {
            if (props == null)
            {
                Debug.LogError("SessionProps is null");
                return;
            }    

            foreach (FieldInfo field in GetType().GetFields())
            {
                field.SetValue(this, ConvertFromSessionProp(props[field.Name], field.FieldType));
            }
        }

        public Dictionary<string, SessionProperty> Properties
        {
            get
            {
                Dictionary<string, SessionProperty> props = new Dictionary<string, SessionProperty>();
                foreach (FieldInfo field in GetType().GetFields())
                {
                    props[field.Name] = ConvertToSessionProp(field.GetValue(this));
                }
                return props;
            }
        }

        private object ConvertFromSessionProp(SessionProperty sp, Type toType)
        {
            if (toType == typeof(bool))
                return (int)sp == 1;
            if (sp.IsString)
                return (string)sp;
            return (int)sp;
        }

        private SessionProperty ConvertToSessionProp(object value)
        {
            if (value is string)
                return SessionProperty.Convert(value);
            if (value is bool b)
                return b ? 1 : 0;
            return (int)value;
        }
    }
}