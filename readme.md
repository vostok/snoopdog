# Dump analyzer

## Checklist

|                                           |dmp impl   |live impl  |clrmd can  |impl hints |windbg hints
| -----------------------------             |:---------:|:---------:|:---------:|:--------- |:-----------
|__statistics__                             |           |           |           |           |            
|&nbsp;&nbsp;top 10 consuming types         |+          |           |+          |           |            
|__problems__                               |           |           |           |           |            
|&nbsp;&nbsp;__memory__                     |           |           |           |           |            
|&nbsp;&nbsp;&nbsp;&nbsp;managed memory leak|+-         |           |+          |           |            
|&nbsp;&nbsp;&nbsp;&nbsp;high consumption   |           |           |?          |           |            
|&nbsp;&nbsp;__threads__                    |           |           |           |           |            
|&nbsp;&nbsp;&nbsp;&nbsp;deadlocks          |+          |+          |+          |           |!dlk        
|&nbsp;&nbsp;&nbsp;&nbsp;lock convoy        |+          |+          |+          |           |            
|&nbsp;&nbsp;&nbsp;&nbsp;inf loop           |           |           |           |           |            
|&nbsp;&nbsp;__crash__                      |           |           |           |           |            
|&nbsp;&nbsp;&nbsp;&nbsp;.NET exception     |           |           |           |           |            
|&nbsp;&nbsp;&nbsp;&nbsp;stack overflow     |           |           |           |           |            
|&nbsp;&nbsp;&nbsp;&nbsp;access violation   |           |           |           |           |            
|__metrics__                                |           |           |           |           |            
|&nbsp;&nbsp;commit size                    |           |           |-          |!address   |            
|&nbsp;&nbsp;working set                    |           |           |-          |!address   |            
|&nbsp;&nbsp;private bytes                  |           |           |-          |!address   |            
|&nbsp;&nbsp;heap size                      |           |           |           |           |            
|&nbsp;&nbsp;&nbsp;&nbsp;gen 0, 1, 2, LOH   |+          |+          |+          |           |            
|&nbsp;&nbsp;threads count                  |+          |+          |+          |           |            
    