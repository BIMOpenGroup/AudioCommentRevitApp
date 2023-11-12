using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using System;

namespace AudioAddin
{
    public class ExtensibleStorageUtils
    {

        private Guid _schemaGuid;
        private string _vendorId;
        private string _schemaName;
        private string _soundMessageFieldName;
        private string _usersFieldName;

        public ExtensibleStorageUtils(string schemaGuid, string vendorId, string schemaName, string soundMessageFildName, string usersFieldName)
        {
            _schemaGuid = new Guid(schemaGuid);
            _vendorId = vendorId;
            _schemaName = schemaName;
            _soundMessageFieldName = soundMessageFildName;
            _usersFieldName = usersFieldName;
        }

        public string GetSoundMessage(Document doc, ElementId elementId)
        {
            return GetElementExtensibleStorageField(doc, elementId, _soundMessageFieldName);
        }

        public string GetUsersList(Document doc, ElementId elementId)
        {
            return GetElementExtensibleStorageField(doc, elementId, _usersFieldName);
        }

        public string GetElementExtensibleStorageField(Document doc, ElementId elementId, string fieldName)
        {
            Element element = doc.GetElement(elementId);

            // Проверяем наличие Extensible Storage
            if (element.IsValidObject)
            {
                var schema = GetSchema(_schemaGuid);
                if (schema != null)
                {
                    //Schema schema = GetOrCreateSchema(_schemaGuid);
                    Entity entity = GetEntity(element, schema);
                    // Проверяем наличие поля
                    if (entity.IsValid())
                    {
                        Field field = schema.GetField(fieldName);
                        string fieldString = entity.Get<string>(field);
                        if (!string.IsNullOrEmpty(fieldString))
                            return fieldString;
                    }
                }
            }

            return "";
        }

        public void WriteUsersList(Document doc, ElementId elementId, string value)
        {
            WriteToExtensibleStorage(doc, elementId, _usersFieldName, value);
        }

        public void WriteToExtensibleStorage(Document doc, ElementId elementId, string fieldName, string value)
        {
            using (Transaction transaction = new Transaction(doc, "Create sound message"))
            {
                transaction.Start();
                Element element = doc.GetElement(elementId);

                // Получаем или создаем Extensible Storage схему
                Schema schema = GetOrCreateSchema(_schemaGuid);

                // Получаем или создаем сущность
                Entity entity = GetOrCreateEntity(element, schema);

                // Записываем значение в поле
                entity.Set(fieldName, value);

                // Сохраняем изменения
                SaveEntity(element, entity);
                transaction.Commit();
            }

        }
        public Schema GetSchema(Guid schemaGuid)
        {
            return Schema.Lookup(schemaGuid);
        }

        public Schema GetOrCreateSchema(Guid schemaGuid)
        {
            Schema schema = Schema.Lookup(schemaGuid);

            if (schema == null)
            {
                SchemaBuilder schemaBuilder = new SchemaBuilder(schemaGuid);
                schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
                schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
                schemaBuilder.SetVendorId(_vendorId);
                schemaBuilder.SetSchemaName(_schemaName);
                FieldBuilder fieldBuilderSoundMessage = schemaBuilder.AddSimpleField(_soundMessageFieldName, typeof(string));
                fieldBuilderSoundMessage.SetDocumentation("A stored location value representing a bytes of soundmessage");
                FieldBuilder fieldBuilderUsersWhoListened = schemaBuilder.AddSimpleField(_usersFieldName, typeof(string));
                fieldBuilderUsersWhoListened.SetDocumentation("Sequence of users who listened to this audio message");
                schema = schemaBuilder.Finish();
            }
            return schema;
        }

        public Entity GetOrCreateEntity(Element element, Schema schema)
        {
            Entity entity = null;

            if (element.IsValidObject && schema != null)
            {
                if (element.GetEntity(schema) != null)
                {
                    entity = element.GetEntity(schema);
                }
                else
                {
                    entity = new Entity(schema);
                    element.SetEntity(entity);
                }
            }

            return entity;
        }

        public Entity GetEntity(Element element, Schema schema)
        {
            if (element.IsValidObject && schema != null)
            {
                return element.GetEntity(schema);
            }

            return null;
        }

        public void SaveEntity(Element element, Entity entity)
        {
            if (element.IsValidObject && entity != null)
            {
                element.SetEntity(entity);
            }
        }

        public void StoreSoundDataInElement(Element elem, byte[] bytes)
        {
            using (Transaction transaction = new Transaction(elem.Document, "Create sound message"))
            {
                transaction.Start();

                Schema schema = GetOrCreateSchema(_schemaGuid);
                Entity entity = new Entity(schema);
                Field fieldSpliceLocation = schema.GetField(_soundMessageFieldName);
                var stringSound = Convert.ToBase64String(bytes);
                entity.Set<string>(fieldSpliceLocation, stringSound);
                elem.SetEntity(entity);

                transaction.Commit();
            }
        }
    }
}
