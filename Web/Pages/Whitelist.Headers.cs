using System.Collections.Generic;
using API.Component;
using Subsegment.Bits;
using Subsegment.Constructs;

namespace Web.Pages
{
    public partial class Whitelist
    {
        private Subheader? _ruleSubheader;
        private Subheader? _listSubheader;

        private void BuildHeaders()
        {
            _ruleSubheader = new Subheader(new List<IBit>
            {
                new Space(10)
            });

            if (!_editing)
            {
                _ruleSubheader.Add(new Title("Create a whitelist rule"));
            }
            else
            {
                _ruleSubheader.Add(new Title("Edit whitelist rule"));
                _ruleSubheader.Add(new Space());
                _ruleSubheader.Add(new MonoBlock(Rule().ID.ToString()));
                _ruleSubheader.Add(new Space(15));
                _ruleSubheader.Add(new Separator());
                _ruleSubheader.Add(new Space(15));
                _ruleSubheader.Add(new Button("CANCEL", CancelEdit, Button.Color.Red));
            }

            //

            _listSubheader = new Subheader(
                new List<IBit>
                {
                    new Space(10),
                    new Title("Existing whitelist rules")
                },
                borderTop: true
            );
        }
    }
}