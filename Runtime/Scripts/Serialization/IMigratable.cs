/*
 *	Created by:  Peter @sHTiF Stefcek
 */

namespace Nodemon
{
    public interface IMigratable
    {
        void Migrate(IMigratable p_previousVersion);
    }
}