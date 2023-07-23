const table = "Input"
const version = 1
Database = {
    write: writeData
}
function writeData(data) {
    return new Promise((resolve,reject) => {
        const write= (db) => {
            const trans = db.transaction(table, "readwrite")
            const objectStore = trans.objectStore(table)
            trans.onerror = reject
            const req = objectStore.add(data)
            req.onsuccess = e => resolve(e.target.result)
        }
        const req = indexedDB.open(table, version)
        req.onerror = reject
        req.onupgradedneeded = e => {
            const db = e.target.result
            const objectStore = 
                db.createObjectStore(table, {autoIncrement: true})
            objectStore.createIndex(table, "session", {unique: false})
            objectStore.transaction.oncomplete = () => write(db)
        }
        req.onsuccess = e => write(e.target.result)
    })
}
