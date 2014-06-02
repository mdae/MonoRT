/*
 * rt-gc.c: GC implementation 0.2
 *
 */

#include "config.h"
#include "exception.h"
#include <glib.h>
#include "metadata.h"
#include "domain-internals.h"
#include <mono/metadata/mono-gc.h>
#include <mono/metadata/gc-internal.h>
#include <pthread.h>
#ifdef HAVE_RT_GC


//----> CHANGED ON 9.9.2011 15:41

//Local Storages for the Hash-Tables
pthread_key_t enableSavingTLS=0,tableTLS=0,memberVarsTableTLS=0,staticVarsTableTLS=0;
static pthread_mutex_t tableMutex=PTHREAD_MUTEX_INITIALIZER;
static pthread_mutex_t memberVarsTableMutex=PTHREAD_MUTEX_INITIALIZER;
static pthread_mutex_t staticVarsTableMutex=PTHREAD_MUTEX_INITIALIZER;

//global hash index for all tables
int hash_index=0;

//Checks if the thread is in snapshot mode
int isGCSavingEnabled(){

	int saving=pthread_getspecific(enableSavingTLS);

	return saving;

}

//Set thread snapshot state
void setSavingEnabled(int value){
pthread_setspecific(enableSavingTLS,value);
}


//Allocates memory for the Object and saving its adress in the table
void*
mono_gc_alloc_obj(MonoVTable *vtable, size_t size) {
	void *res;
	res = calloc(1, (size));
	((MonoObject*) res)->vtable = vtable;
	if (mono_rtgc_dbg)
		printf("mono_gc_alloc_obj %d,%d\n", size, res);
	if (isGCSavingEnabled()== 1) {
		GHashTable *table=pthread_getspecific(tableTLS);
		pthread_mutex_lock(&tableMutex);
		g_hash_table_insert(table, hash_index, res);
		hash_index++;
		pthread_mutex_unlock(&tableMutex);
	}
	return res;
}


//restores an object
static int restore_object_pointer(gpointer key, gpointer value) {
	if (mono_rtgc_dbg)
		printf("restoring: %d with value %d\n", ((int*) key), value);

	*((int*) key) = value;
	return TRUE;
}

//deletes object from heap and call its finalizer
static int delete_object_from_heap(gpointer key, gpointer value) {
	if (mono_rtgc_dbg)
		printf("%d,%d\n", key, value);
	mono_gc_run_finalize(value, NULL);
	free(value);
	return TRUE;
}

// GC-Init. First called function. creates thread local sotrages
void mono_gc_base_init(void) {
	if (mono_rtgc_dbg)
		printf("gc_base_init\n");
	
//Init local storages for threads
pthread_key_create(&tableTLS, NULL);
pthread_key_create(&enableSavingTLS,NULL);

pthread_key_create(&memberVarsTableTLS, NULL);
pthread_key_create(&staticVarsTableTLS, NULL);


setSavingEnabled(0);
	hash_index = 0;

}

//Stores the old content-pointer of a static variable if in snapshot mode
void rtgcStoreStaticValue(MonoDomain *domain, MonoClassField *field) {
	MonoVTable *vtable;
	gpointer addr;

	MONO_ARCH_SAVE_REGS;
	if (isGCSavingEnabled()) {

		mono_class_init(field->parent);
		GHashTable *table=pthread_getspecific(tableTLS);
		GHashTable *staticVarsTable=pthread_getspecific(staticVarsTableTLS);
		vtable = mono_class_vtable_full(domain, field->parent, TRUE);
		if (!vtable->initialized)
			mono_runtime_class_init(vtable);

		if (domain->special_static_fields && (addr = g_hash_table_lookup(
				domain->special_static_fields, field)))
			addr = mono_get_special_static_data(GPOINTER_TO_UINT(addr));
		else
			addr = (char*) vtable->data + field->offset;
		pthread_mutex_lock(&staticVarsTableMutex);
		if (g_hash_table_lookup_extended(staticVarsTable, addr, NULL, NULL)
				!= TRUE)
			g_hash_table_insert(staticVarsTable, addr, *((int*) addr));
		pthread_mutex_unlock(&staticVarsTableMutex);
		if (mono_rtgc_dbg) {
			printf("rtgStoreSTATICValue %d @ %d\n", *((int*) addr), addr);
		}
	}
}

//Stores the non-static member content-pointer if in snapshot mode
void rtgcStoreValue(gpointer objaddr, MonoClassField *field) {
	MonoVTable *vtable;
	gpointer addr;

	MONO_ARCH_SAVE_REGS;
	if (isGCSavingEnabled()) {
		GHashTable *memberVarsTable=pthread_getspecific(memberVarsTableTLS);
		addr = ((char*) objaddr) + field->offset;
		pthread_mutex_lock(&memberVarsTableMutex);
		if (g_hash_table_lookup_extended(memberVarsTable, addr, NULL, NULL)
				!= TRUE) {
			//printf("insterting %d to memberhashtable\n",addr);
			g_hash_table_insert(memberVarsTable, addr, *((int*) addr));
		}
		pthread_mutex_unlock(&memberVarsTableMutex);
	}
	if (mono_rtgc_dbg) {
		addr = ((char*) objaddr) + field->offset;
		printf("rtgStoreValue %d\ @ %d name:%s \n ", *((int*) addr), addr,
				field->name);
		//		printf("rtgStoreValue %d name:%s \n ",addr,field->name);
	}
}


//Main collect function. Contorols the Snapshot-GC
void mono_gc_collect(int generation) {
	if (mono_rtgc_dbg)
		printf("gc_collect %d\n", generation);
	//Snapshot mode enable.
	if (generation == 0xBEAF) {
	GHashTable *table, *memberVarsTable, *staticVarsTable;
	table=pthread_getspecific(tableTLS);
		//If there are no hash tables. create them
		if(table==NULL){
			table = g_hash_table_new(NULL, NULL);
			memberVarsTable = g_hash_table_new(g_direct_hash, NULL);
			staticVarsTable = g_hash_table_new(NULL, NULL);
			pthread_setspecific(tableTLS,table);
			pthread_setspecific(memberVarsTableTLS,memberVarsTable);
			pthread_setspecific(staticVarsTableTLS,staticVarsTable);
		}
	setSavingEnabled(1);
	}
	//Collect the garbage
	if (generation == 0xDEAD) {
		GHashTable *table=pthread_getspecific(tableTLS);
		GHashTable *staticVarsTable=pthread_getspecific(staticVarsTableTLS);
		GHashTable *memberVarsTable=pthread_getspecific(memberVarsTableTLS);
		//RESTORE MEMBER VARS
		pthread_mutex_lock(&memberVarsTableMutex);
		g_hash_table_foreach_remove(memberVarsTable,
				(GHFunc) restore_object_pointer, NULL);
		pthread_mutex_unlock(&memberVarsTableMutex);
		//RESTORE STATUC VARS
		pthread_mutex_lock(&staticVarsTableMutex);
		g_hash_table_foreach_remove(staticVarsTable,
				(GHFunc) restore_object_pointer, NULL);
		pthread_mutex_unlock(&staticVarsTableMutex);
		//DELETE ALL NEW OBJECTS CREATED AFTER SNAPSHOT START
		pthread_mutex_lock(&tableMutex);
		g_hash_table_foreach_remove(table, (GHFunc) delete_object_from_heap,
				NULL);
		pthread_mutex_unlock(&tableMutex);

	}
	//ONLY FOR DEBUG! PLEASE DONT USE!
	if (generation == 0xD42) {
		GHashTable *table=pthread_getspecific(tableTLS);
		GHashTable *staticVarsTable=pthread_getspecific(staticVarsTableTLS);
		GHashTable *memberVarsTable=pthread_getspecific(memberVarsTableTLS);
		pthread_mutex_lock(&memberVarsTableMutex);
		g_hash_table_foreach_remove(memberVarsTable,
				(GHFunc) restore_object_pointer, NULL);
		pthread_mutex_unlock(&memberVarsTableMutex);
		g_hash_table_foreach_remove(staticVarsTable,
				(GHFunc) restore_object_pointer, NULL);

	}
}



//<----CHANGED ON 9.9.2011 15:41

int mono_gc_max_generation(void) {
	if (mono_rtgc_dbg)
		printf("gc_max_generation\n");
	return 0;
}

int mono_gc_get_generation(MonoObject *object) {
	if (mono_rtgc_dbg)
		printf("gc_get_generation\n");
	return 0;
}

int mono_gc_collection_count(int generation) {
	if (mono_rtgc_dbg)
		printf("gc_collection_count\n");
	return 0;
}

void mono_gc_add_memory_pressure(gint64 value) {
	if (mono_rtgc_dbg)
		printf("gc_add_memory_pressure\n");
}

/* maybe track the size, not important, though */
gint64 mono_gc_get_used_size(void) {
	if (mono_rtgc_dbg)
		printf("gc_get_used_size\n");
	return 1024 * 1024;
}

gint64 mono_gc_get_heap_size(void) {
	if (mono_rtgc_dbg)
		printf("gc_heap_size\n");
	return 2 * 1024 * 1024;
}

void mono_gc_disable(void) {
	if (mono_rtgc_dbg)
		printf("gc_disable\n");
}

void mono_gc_enable(void) {
	if (mono_rtgc_dbg)
		printf("gc_enable\n");
}

gboolean mono_gc_is_gc_thread(void) {
		
	return TRUE;
}

gboolean mono_gc_register_thread(void *baseptr) {
	
	return TRUE;
}

gboolean mono_object_is_alive(MonoObject* o) {
	return TRUE;
}

void mono_gc_enable_events(void) {
	if (mono_rtgc_dbg)
		printf("gc_enable_events\n");
}

int mono_gc_register_root(char *start, size_t size, void *descr) {
	if (mono_rtgc_dbg)
		printf("gc_register_root\n");
	return TRUE;
}

void mono_gc_deregister_root(char* addr) {
	if (mono_rtgc_dbg)
		printf("gc_deregister_root\n");
}

void mono_gc_weak_link_add(void **link_addr, MonoObject *obj, gboolean track) {
	if (mono_rtgc_dbg)
		printf("gc_weak_link_add\n");
	*link_addr = obj;
}

void mono_gc_weak_link_remove(void **link_addr) {
	if (mono_rtgc_dbg)
		printf("gc_weak_link_remove\n");
	*link_addr = NULL;
}

MonoObject*
mono_gc_weak_link_get(void **link_addr) {
	if (mono_rtgc_dbg)
		printf("gc_weak_link_get\n");
	return *link_addr;
}

void*
mono_gc_make_descr_for_string(gsize *bitmap, int numbits) {
	if (mono_rtgc_dbg)
		printf("gc_make_descr_for_string\n");
	return NULL;
}

void*
mono_gc_make_descr_for_object(gsize *bitmap, int numbits, size_t obj_size) {
	if (mono_rtgc_dbg)
		printf("gc_make_descr_for_object\n");
	return NULL;
}

void*
mono_gc_make_descr_for_array(int vector, gsize *elem_bitmap, int numbits,
		size_t elem_size) {
	if (mono_rtgc_dbg)
		printf("gc_make_descr_for_array\n");
	return NULL;
}

void*
mono_gc_make_descr_from_bitmap(gsize *bitmap, int numbits) {
	if (mono_rtgc_dbg)
		printf("gc_make_descr_for_bitmap\n");
	return NULL;
}

void*
mono_gc_alloc_fixed(size_t size, void *descr) {
	if (mono_rtgc_dbg)
		printf("gc_alloc_fixed\n");
	return g_malloc0(size);
}

void mono_gc_free_fixed(void* addr) {
	if (mono_rtgc_dbg)
		printf("gc_free_fixed\n");
	g_free(addr);
}

void mono_gc_wbarrier_set_field(MonoObject *obj, gpointer field_ptr,
		MonoObject* value) {
	if (mono_rtgc_dbg)
		printf("gc_wbarrier_set_field\n");
	*(void**) field_ptr = value;
}

void mono_gc_wbarrier_set_arrayref(MonoArray *arr, gpointer slot_ptr,
		MonoObject* value) {
	if (mono_rtgc_dbg)
		printf("%d gc_wbarrier_set_arrayref\n", mono_rtgc_dbg);
	*(void**) slot_ptr = value;
}

void mono_gc_wbarrier_arrayref_copy(MonoArray *arr, gpointer slot_ptr,
		int count) {
	if (mono_rtgc_dbg)
		printf("%d mono_gc_wbarrier_arrayref_copy\n", mono_rtgc_dbg);
	/* no need to do anything */
}

void mono_gc_wbarrier_generic_store(gpointer ptr, MonoObject* value) {
	if (mono_rtgc_dbg)
		printf("%d mono_gc_wbarrier_generic_store\n", mono_rtgc_dbg);
	*(void**) ptr = value;
}

void mono_gc_wbarrier_generic_nostore(gpointer ptr) {
	if (mono_rtgc_dbg)
		printf("%d mono_gc_wbarrier_generic_nostore \n", mono_rtgc_dbg);
}

void mono_gc_wbarrier_value_copy(gpointer dest, gpointer src, int count,
		MonoClass *klass) {
	if (mono_rtgc_dbg)
		printf("%d mono_gc_wbarrier_value_copy  \n", mono_rtgc_dbg);
}

void mono_gc_wbarrier_object(MonoObject* obj) {

	if (mono_rtgc_dbg)
		printf("%d mono_gc_wbarrier_object  \n", mono_rtgc_dbg);

}

MonoMethod*
mono_gc_get_managed_allocator(MonoVTable *vtable, gboolean for_box) {
	if (mono_rtgc_dbg)
		printf("%d mono_gc_get_managed_allocator  \n", mono_rtgc_dbg);
	return NULL;
}

int mono_gc_get_managed_allocator_type(MonoMethod *managed_alloc) {
	if (mono_rtgc_dbg)
		printf("%d mono_gc_get_managed_allocator_type  \n", mono_rtgc_dbg);
	return -1;
}

MonoMethod*
mono_gc_get_managed_allocator_by_type(int atype) {
	if (mono_rtgc_dbg)
		printf("%d mono_gc_get_managed_allocator_by_type \n", mono_rtgc_dbg);
	return NULL;
}

guint32 mono_gc_get_managed_allocator_types(void) {
	if (mono_rtgc_dbg)
		printf("%d mono_gc_get_managed_allocator_types  \n", mono_rtgc_dbg);
	return 0;
}

void mono_gc_add_weak_track_handle(MonoObject *obj, guint32 gchandle) {
	if (mono_rtgc_dbg)
		printf("gc_add_weak_track_handle\n");
}

void mono_gc_change_weak_track_handle(MonoObject *old_obj, MonoObject *obj,
		guint32 gchandle) {
	if (mono_rtgc_dbg)
		printf("gc_change_weak_track_handle\n");
}

void mono_gc_remove_weak_track_handle(guint32 gchandle) {
	if (mono_rtgc_dbg)
		printf("gc_remove_weak_track_handle\n");
}

GSList*
mono_gc_remove_weak_track_object(MonoDomain *domain, MonoObject *obj) {
	if (mono_rtgc_dbg)
		printf("gc_remove_weak_track_object\n");
	return NULL;
}

void mono_gc_clear_domain(MonoDomain *domain) {
	if (mono_rtgc_dbg)
		printf("gc_clear_domain\n");
}

#endif

